using local_network_chat_app.Model;
using local_network_chat_app.Services;
using local_network_chat_app.ViewModel;
using local_network_chat_app.WindowForm;
using System.Net;
using System;
using System.Windows;
using System.Linq;

namespace local_network_chat_app
{
    public partial class MainWindow : Window
    {
        private readonly ConnectionHelper _conn;
        private bool isConnecting = false;
        private bool isServer = false;

        public MainWindow()
        {
            InitializeComponent();
            _conn = ConnectionHelper.Instance;
            _conn.OnClientConnectionStatusChanged += (isConnected) =>
            {
                if (isConnected && progressContainer.Visibility == Visibility.Visible)
                {
                    progressContainer.Visibility = Visibility.Collapsed;
                    OpenChatForm();
                }
            };
           //_conn.StartServer(); //Remove this later. For testing only.
           //_conn.StartClient(); //Remove this later. For testing only.
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (isConnecting)
            {
                StopConnection();
                return;
            }

            isConnecting = true;
            progressContainer.Visibility = Visibility.Visible;
            radioButtonGroup.IsEnabled = false;
            continueButton.Content = "Stop";

            if ((bool)radioButton1.IsChecked)
            {
                _conn.StartServer();
                isServer = true;
                User newClient = new(){
                    IPAddress = _conn.GetServerIP(),
                    UserType = "Server"
                };
                UserViewModel.Instance.Users.Add(newClient);
                OpenChatForm();
            }
            else if ((bool)radioButton2.IsChecked)
            {
                _conn.StartClient();
            }
        }

        private void StopConnection()
        {
            isConnecting = false;
            radioButtonGroup.IsEnabled = true;
            continueButton.Content = "Continue";
            progressContainer.Visibility = Visibility.Collapsed;
        }

        private void OpenChatForm()
        {
            this.Hide();
            ChatWindow chatWindow = new ChatWindow(isServer);
            chatWindow.Closed += (s, e) =>
            {
                _conn.Dispose();
                System.Windows.Application.Current.Shutdown();
            };
            chatWindow.Show();
        }
    }
}
