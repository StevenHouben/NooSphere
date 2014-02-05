namespace NooSphere.Infrastructure.Web
{
    public class NooMessage
    {
        public string Header { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public object Content { get; set; }
        public string[] Extensions { get; set; }
        public MessageType Type { get; set; }

        public NooMessage()
        {
            Type = MessageType.Communication;
        }
    }

    public class BulkMessage : NooMessage
    {
        public NooMessage[] Bulk { get; set; }
    }

    public enum MessageType
    {
        Connect,
        Control,
        Device,
        Communication,
        Notification,
        Custom,
        Resource,
        ResourceRemove,
        ActivityChanged
    }
}