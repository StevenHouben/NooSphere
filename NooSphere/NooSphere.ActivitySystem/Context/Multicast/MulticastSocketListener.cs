/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;

namespace NooSphere.ActivitySystem.Context.Multicast
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