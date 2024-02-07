using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace local_network_chat_app.Services
{
    public class TServer : IDisposable
    {
        public string ServerResponse { get; set; } = "i-am-the-server";
        public string ClientRequest { get; set; } = "looking-for-server";
        public int Port { get; set; } = 5597; // Default port

        private TcpListener? _server;
        private List<TcpClient> _clients = new List<TcpClient>();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public event Action<Exception>? ExceptionOccurred;
        public event Action<TcpClient, string>? OnMessageReceived;
        public event Action<TcpClient>? OnNewClientAdded;
        public event Action<TcpClient, string>? OnClientDisconnected;

        public string GetServerIP() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var serverIP = "Unknown";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    serverIP = ip.ToString();
                }
            }
            return serverIP;
        }

        public async Task StartServer()
        {
            _server = new TcpListener(IPAddress.Any, Port);
            _server.Start();
            System.Diagnostics.Debug.WriteLine($"TCP | SERVER STARTED!!");
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    var bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                    var message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    System.Diagnostics.Debug.WriteLine($"TCP | MESSAGE FROM CLIENT {message}");
                    if (message == ClientRequest)
                    {
                        _clients.Add(client);
                        OnNewClientAdded?.Invoke(client);
                        var response = Encoding.ASCII.GetBytes(ServerResponse);
                        await stream.WriteAsync(response, 0, response.Length);
                        _ = HandleClient(client, stream); // Start a new task to handle this client
                    }
                    else
                    {
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"TCP | {ex}");
                    HandleException("StartServer", ex);
                }
            }
        }

        private async Task HandleClient(TcpClient client, NetworkStream stream)
        {
            IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            string clientIP = remoteIpEndPoint?.Address.ToString();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            while (client.Connected && !_cts.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                if (bytesRead == 0)
                {
                    // Client has closed the connection
                    _clients.Remove(client);
                    client.Close();
                    OnClientDisconnected?.Invoke(client, clientIP);
                    return;
                }
                var message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                HandleMessageReceived(client, message);
            }
        }

        public async Task SendMessage(TcpClient client, string message)
        {
            if (client.Connected)
            {
                var stream = client.GetStream();
                var data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        public async Task SendMessageToAll(string message)
        {
            System.Diagnostics.Debug.WriteLine($"Sending message to all clients: {message}");
            foreach (var client in _clients)
            {
                await SendMessage(client, message);
            }
        }

        public async Task SendMessageExcept(TcpClient exceptClient, string message)
        {
            foreach (var client in _clients)
            {
                if (client != exceptClient)
                {
                    await SendMessage(client, message);
                }
            }
        }

        private async void HandleMessageReceived(TcpClient client, string message)
        {
            System.Diagnostics.Debug.WriteLine($"Message received from Client: {message}");
            OnMessageReceived?.Invoke(client, message);
            await SendMessageExcept(client, message);
        }

        private void HandleException(string methodName, Exception ex)
        {
            if (ex is SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine($"Exception Occured | {methodName} : {socketException.SocketErrorCode}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Exception Occured | {methodName} : {ex.Message}");
            }
            ExceptionOccurred?.Invoke(ex);
        }

        public void StopServer()
        {
            if (_cts != null && !_cts.IsCancellationRequested){
                _cts.Cancel();
            }
            foreach (var client in _clients)
            {
                client.Close();
            }
            _clients.Clear();
            _server?.Stop();
        }

        public void Dispose()
        {
            this.StopServer();
            _cts.Dispose();
            _server?.Stop();
        }
    }
}
