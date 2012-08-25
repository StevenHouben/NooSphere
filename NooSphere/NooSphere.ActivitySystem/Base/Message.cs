/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

namespace NooSphere.ActivitySystem.Base
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
    public class BulkMessage:Message
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
