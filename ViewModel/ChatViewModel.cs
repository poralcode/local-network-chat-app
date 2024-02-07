using local_network_chat_app.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace local_network_chat_app.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private static ChatViewModel _instance;
        public static ChatViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ChatViewModel();
                }
                return _instance;
            }
        }

        private ObservableCollection<Chat> _messages;
        public ObservableCollection<Chat> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged("Messages");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ChatViewModel() // Make the constructor private
        {
            _messages = new ObservableCollection<Chat>();
        }
    }
}
