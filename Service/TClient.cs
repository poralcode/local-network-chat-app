using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace local_network_chat_app.Services
{
    public class TClient : IDisposable
    {
        public string ServerResponse { get; set; } = "i-am-the-server";
        public string ClientRequest { get; set; } = "looking-for-server";
        public int Port { get; set; } = 5597;
        public int Timeout { get; set; } = 3000;

        private TcpClient? _client;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public bool IsConnected { get; private set; }

        public event Action<Exception>? ExceptionOccurred;
        public event Action<string>? OnMessageReceived;
        public event Action<bool>? OnClientConnectionStatusChanged;

        public async Task StartClient(string serverIp)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(serverIp, Port);

                var stream = _client.GetStream();
                await stream.WriteAsync(Encoding.ASCII.GetBytes(ClientRequest), 0, ClientRequest.Length);

                var buffer = new byte[_client.ReceiveBufferSize];
                var bytesRead = await stream.ReadAsync(buffer, 0, _client.ReceiveBufferSize, new CancellationTokenSource(Timeout).Token);

                if (bytesRead == 0 || Encoding.ASCII.GetString(buffer, 0, bytesRead) != ServerResponse)
                {
                    ConnectionStatusChanged(false);
                    _client.Close();
                    _client = null;
                    return;
                }

                IsConnected = true;
                ConnectionStatusChanged(true);
                _ = HandleServerMessages(stream);
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged(_client != null && !_client.Connected);
                HandleException(ex);
            }
        }

        private async Task HandleServerMessages(NetworkStream stream)
        {
            var buffer = new byte[_client.ReceiveBufferSize];
            while (_client?.Connected == true && !_cts.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, _client.ReceiveBufferSize);
                if (bytesRead == 0)
                {
                    // Server has closed the connection
                    StopClient();
                    return;
                }
                HandleMessageReceived(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
        }

        private void ConnectionStatusChanged(bool isConnected)
        {
            System.Diagnostics.Debug.WriteLine($"Client connection status change: {isConnected}");
            IsConnected = isConnected;
            OnClientConnectionStatusChanged?.Invoke(isConnected);
        }

        private void HandleMessageReceived(string message)
        {
            System.Diagnostics.Debug.WriteLine($"Message recieved from server: {message}");
            OnMessageReceived?.Invoke(message);
        }

        public async Task SendMessage(string message)
        {
            if (_client?.Connected == true)
            {
                await _client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(message), 0, message.Length);
            }
            else
            {
                throw new InvalidOperationException("Client is not connected to a server.");
            }
        }

        private void HandleException(Exception ex)
        {
            var errorMessage = ex is SocketException socketException ? $"Error while receiving: {socketException.SocketErrorCode}" : $"Error while receiving: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            ExceptionOccurred?.Invoke(ex);
        }

        public void StopClient()
        {
            _client?.Close();
            _client = null;
            IsConnected = false;
            ConnectionStatusChanged(false);
        }

        public void Dispose()
        {
            StopClient();
            _cts.Dispose();
        }
    }
}
