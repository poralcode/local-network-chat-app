using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace local_network_chat_app.Services
{
    public class Udp : IDisposable
    {
        public string ServerResponse { get; set; } = "i-am-the-server";
        public string ClientRequest { get; set; } = "looking-for-server";
        public int Timeout { get; set; } = 3000; // Timeout in milliseconds
        public int Port { get; set; } = 5597; // Default port

        private UdpClient? _client;
        private UdpClient? _server;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public event Action<Exception>? ExceptionOccurred;

        public async Task StartServer()
        {
            _server = new UdpClient(Port);

            var responseData = Encoding.ASCII.GetBytes(ServerResponse);
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var result = await _server.ReceiveAsync();
                    var clientRequestData = result.Buffer;
                    var clientEp = result.RemoteEndPoint; // Use the endpoint from the received data
                    var clientRequest = Encoding.ASCII.GetString(clientRequestData);
                    if (clientRequest == ClientRequest)
                    {
                        System.Diagnostics.Debug.WriteLine($"Server: Received valid request from {clientEp.Address}, sending response");
                        _server.Send(responseData, responseData.Length, clientEp); // Send the response to the client's endpoint
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        public void StopServer(  )
        {
            try
            {
                _cts.Cancel();
                _server?.Close();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public async Task<string?> StartClient()
        {
            while (!_cts.IsCancellationRequested)
            {
                using (_client = new UdpClient())
                {
                    var requestData = Encoding.ASCII.GetBytes(ClientRequest);
                    _client.EnableBroadcast = true;
                    _client.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, Port));

                    try
                    {
                        var receiveTask = _client.ReceiveAsync();
                        var delayTask = Task.Delay(Timeout);
                        var completedTask = await Task.WhenAny(receiveTask, delayTask);

                        if (completedTask == receiveTask)
                        {
                            var serverResponseDataResult = await receiveTask;
                            var serverResponse = Encoding.ASCII.GetString(serverResponseDataResult.Buffer);
                            if (serverResponse == ServerResponse)
                            {
                                return serverResponseDataResult.RemoteEndPoint.Address.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
            }
            return null;
        }
        private void HandleException(Exception ex)
        {
            if (ex is SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine($"Error while receiving: {socketException.SocketErrorCode}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Error while receiving: {ex.Message}");
            }
            ExceptionOccurred?.Invoke(ex);
        }

        public void StopClient()
        {
            try
            {
                _cts.Cancel();
                _client?.Close();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void Dispose()
        {
            this.StopServer();
            this.StopClient();
            _cts.Dispose();
            _server?.Dispose();
        }
    }
}
