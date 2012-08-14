using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NooSphere.ActivitySystem.Context
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
