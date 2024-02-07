using System;

namespace local_network_chat_app.Model
{
    public class Chat
    {
        public string Sender { get; set; } = "Unknown Sender";
        public string IPAddress { get; set; } = "Unknown IP Address";
        public string UserType { get; set; } = "Unknown User Type";
        public string Message { get; set; } = "+nknown Message";
        public string MessageType { get; set; } = "Unknown Type";
        public DateTime? DateSent { get; set; } = DateTime.Now;
    }

}
