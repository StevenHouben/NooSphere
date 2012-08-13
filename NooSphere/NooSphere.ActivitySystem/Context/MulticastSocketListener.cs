using System;

namespace NooSphere.ActivitySystem.Context
{
    public interface IMulticastSocketListener
    {

        void SocketMessage(object sender, NotifyMulticastSocketListenerEventArgs e);

    }

    public enum MulticastSocketMessageType
    {
        SocketStarted,
        MessageReceived,
        ReceiveException,
        MessageSent,
        SendException
    }

    public class NotifyMulticastSocketListenerEventArgs : EventArgs
    {
        public MulticastSocketMessageType Type { get; private set; }

        public object NewObject { get; private set; }

        public int Consecutive { get; private set; }

        public NotifyMulticastSocketListenerEventArgs(MulticastSocketMessageType type, Object newObject)
        {
            Type = type;
            NewObject = newObject;
        }

        public NotifyMulticastSocketListenerEventArgs(MulticastSocketMessageType type, Object newObject, int mCons)
        {
            Type = type;
            NewObject = newObject;
            Consecutive = mCons;
        }
    }

    public delegate void NotifyMulticastSocketListener(object sender, NotifyMulticastSocketListenerEventArgs e);

}