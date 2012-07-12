/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NooSphere.Helpers
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
        public static Uri GetUrl(string ip, int port, string relative)
        {
            return new Uri(string.Format("http://{0}:{1}/{2}", ip, port, relative));
        }
    }
}
