using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NooSphere.ActivitySystem.Host
{
    public class NetHelper
    {
        public static int FindPort()
        {
            int port = 0;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
                port = local.Port;
            }

            if (port == 0)
                throw new InvalidOperationException("Unable to find a free port.");

            return port;
        }
        public static string GetIP(bool local)
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (local)
                    {
                        if (ip.ToString().StartsWith("192"))
                        {
                            localIP = ip.ToString();
                            return localIP;
                        }
                        else
                            return ip.ToString();
                    }
                }
            }
            return null;
        }
        public static string NO_IP = "?";
    }
}
