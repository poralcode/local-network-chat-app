using local_network_chat_app.Model;
using local_network_chat_app.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace local_network_chat_app.Services
{
    public class ConnectionHelper
    {
        public string ServerResponse { get; set; } = "i-am-the-server";
        public string ClientRequest { get; set; } = "looking-for-server";
        public int Timeout { get; set; } = 3000; // Timeout in milliseconds
        public int Port { get; set; } = 5597; // Default port
        public string ServerIP { get; set; } = "Unknown";

        private readonly Udp udp;
        private readonly TServer tServer;
        private readonly TClient tClient;
        public event Action<Exception> ExceptionOccurred;
        public event Action<TcpClient, string> OnMessageReceived;
        public event Action<string> OnServerMessage;

        private static ConnectionHelper _instance;
        private static readonly object _lock = new object();
        public event Action<bool>? OnClientConnectionStatusChanged;

        private ConnectionHelper()
        {
            udp = new Udp();
            tServer = new TServer();
            tClient = new TClient();
            udp.ExceptionOccurred += (message) => ExceptionOccurred?.Invoke(message);
            tServer.ExceptionOccurred += (client) => ExceptionOccurred?.Invoke(client);
            tClient.OnClientConnectionStatusChanged += HandleConnectionStatusChanged;
            tServer.OnClientDisconnected += (client, clientIP) =>
            {
                System.Diagnostics.Debug.WriteLine($"TCP | A client has been disconnected: {clientIP}");
                // Find the user with the same IP address
                var userToRemove = UserViewModel.Instance.Users.FirstOrDefault(user => user.IPAddress == clientIP && user.UserType == "Client");
                // If the user is found, remove it from the list
                if (userToRemove != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserViewModel.Instance.Users.Remove(userToRemove);
                    });
                }
            };

            tServer.OnNewClientAdded += (client) =>
            {
                IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                User newClient = new()
                {
                    IPAddress = remoteIpEndPoint?.Address.ToString(),
                    UserType = "Client"
                };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserViewModel.Instance.Users.Add(newClient);
                });
            };

            tServer.OnMessageReceived += (client, message) => {
                System.Diagnostics.Debug.WriteLine($"TCP | Server received a message: {message}");
                IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                Chat msg = new()
                {
                    Message = message,
                    MessageType = "Received",
                    IPAddress = remoteIpEndPoint?.Address.ToString(),
                };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatViewModel.Instance.Messages.Add(msg);
                });
                //composed the message to send to everyone except this client.
                string jsonstring = $"[\"{remoteIpEndPoint?.Address.ToString()}\",\"{message}\"]";
                tServer.SendMessageExcept(client, jsonstring);
            };

            tClient.OnMessageReceived += (message) => {
                List<string> data = JsonConvert.DeserializeObject<List<string>>(message);
                if (data == null) return;
                System.Diagnostics.Debug.WriteLine($"TCP | Client received a message: {message}");
                System.Diagnostics.Debug.WriteLine($"TCP | Client received a message: {data}");
                Chat msg = new()
                {
                    Message = data[1],
                    MessageType = "Received",
                    IPAddress = data[0],
                };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatViewModel.Instance.Messages.Add(msg);
                });
            };
        }
        public static ConnectionHelper Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new ConnectionHelper();
                    return _instance;
                }
            }
        }

        public void StartServer()
        {
            udp.ServerResponse = ServerResponse;
            udp.ClientRequest = ClientRequest;
            udp.Timeout = Timeout;
            udp.Port = Port;

            tServer.ServerResponse = ServerResponse;
            tServer.ClientRequest = ClientRequest;
            tServer.Port = Port;

            Task.Run(() => udp.StartServer());
            Task.Run(() => tServer.StartServer());
        }

        public string GetServerIP()
        {
            return tServer.GetServerIP();
        }

        public void StopServer()
        {
            udp.StopServer();
            tServer.StopServer();
        }

        public async void StartClient()
        {
            udp.ServerResponse = ServerResponse;
            udp.ClientRequest = ClientRequest;
            udp.Timeout = Timeout;
            udp.Port = Port;

            tClient.ServerResponse = ServerResponse;
            tClient.ClientRequest = ClientRequest;
            tClient.Port = Port;
            tClient.Timeout = Timeout;

            ServerIP = await udp.StartClient();
            await tClient.StartClient(ServerIP);
        }

        public void SendMessageToServer(string message)
        {
            Task.Run(() => tClient.SendMessage(message));
        }
        public void SendMessageToClient(string message)
        {
            string jsonstring = $"[\"{tServer.GetServerIP()}\",\"{message}\"]";
            Task.Run(() => tServer.SendMessageToAll(jsonstring));
        }

        /* Handled the status change fo the Client and automatically reconnect to the Server. */
        private void HandleConnectionStatusChanged(bool isConnected)
        {
            if (!isConnected)
                StartClient();
            OnClientConnectionStatusChanged?.Invoke(isConnected);
        } 

        public void StopClient()
        {
            udp.StopClient();
            tClient.StopClient();
        }

        public void Dispose()
        {
            udp?.Dispose();
            tServer?.Dispose();
            tClient?.Dispose();
        }
    }
}
