using local_network_chat_app.Model;
using local_network_chat_app.Services;
using local_network_chat_app.ViewModel;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;

namespace local_network_chat_app.WindowForm
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly ConnectionHelper conn;
        private bool isServer = false;
        public ChatWindow(bool isServer)
        {
            InitializeComponent();
            this.isServer = isServer;
            conn = ConnectionHelper.Instance;
            conn.OnClientConnectionStatusChanged += (isConnected) =>
            {
                if (!isServer) {
                    ChatDialogHost.IsOpen = false;
                    if (!isConnected)
                    {
                        ChatDialogHost.IsOpen = true;
                        System.Diagnostics.Debug.WriteLine($"Connection Change: {isConnected}");
                    }
                }
            };

            if (this.isServer)
            {
                ClientList.Visibility = Visibility.Visible;
            }
            else {
                ConnectedTo.Text = $"Connected to: {conn.ServerIP}";
                ConnectedTo.Visibility = Visibility.Visible;
                Notice.Visibility = Visibility.Collapsed;
                TextBoxMessage.IsEnabled = true;
            }

            UserViewModel.Instance.Users.CollectionChanged += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"client list changes");
                if (UserViewModel.Instance.UserCount > 1)
                {
                    Notice.Visibility = Visibility.Collapsed;
                    TextBoxMessage.IsEnabled = true;
                    TextBoxMessage.Focus();
                }
                else
                {
                    Notice.Visibility = Visibility.Visible;
                    TextBoxMessage.IsEnabled = false;
                }
            };

        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            Chat msg = new()
            {
                Message = TextBoxMessage.Text,
                MessageType = "Sent",
            };
            ChatViewModel.Instance.Messages.Add(msg);
            if (isServer)
            {
                conn.SendMessageToClient(TextBoxMessage.Text);
            }
            else { 
                conn.SendMessageToServer(TextBoxMessage.Text);
            }
        }

        private void TextBoxMessage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Send_Click(sender, e);
                TextBoxMessage.Text = string.Empty;
            }
        }
    }
}
