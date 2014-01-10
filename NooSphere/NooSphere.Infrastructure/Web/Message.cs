namespace ABC.Infrastructure.Web
{
    public class Message
    {
        public string Header { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public string[] Extensions { get; set; }
        public MessageType Type { get; set; }

        public Message()
        {
            Type = MessageType.Communication;
        }
    }

    public class BulkMessage : Message
    {
        public Message[] Bulk { get; set; }
    }

    public enum MessageType
    {
        Connect,
        Control,
        Device,
        Communication,
        Notification,
        Custom
    }
}