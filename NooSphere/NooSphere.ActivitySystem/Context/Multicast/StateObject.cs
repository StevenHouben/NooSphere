/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Net.Sockets;

namespace NooSphere.ActivitySystem.Context.Multicast
{
    public class StateObject
    {
        public const int BufferSize = 1024;

        public byte[] Buffer { get; set; }
        public Socket WorkSocket { get; set; }

        public StateObject()
        {
            Buffer = new byte[BufferSize];
            WorkSocket = null;
        }

        public StateObject(int size, Socket sock)
        {
            Buffer = new byte[size];
            WorkSocket = sock;
        }
    }
}
