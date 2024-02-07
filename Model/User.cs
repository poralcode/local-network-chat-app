
using System;

namespace local_network_chat_app.Model
{
    public class User
    {
        public string Name { get; set; } = "Unknown Name";
        public string IPAddress { get; set; } = "Unknown IP Address";
        public string UserType { get; set; } = "Unknown Type";
        public string Status { get; set; } = "Unknown";
        public string DateConnection { get; set; } = DateTime.Now.ToString();
    }
}
