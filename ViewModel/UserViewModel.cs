using local_network_chat_app.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace local_network_chat_app.ViewModel
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private static UserViewModel _instance;
        public static UserViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserViewModel();
                }
                return _instance;
            }
        }

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
                OnPropertyChanged("UserCount"); // Notify when Users collection changes
            }
        }

        public int UserCount // Add this property
        {
            get { return Users.Count; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private UserViewModel() // Make the constructor private
        {
            _users = new ObservableCollection<User>();
        }
    }
}
