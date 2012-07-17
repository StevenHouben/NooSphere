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

namespace NooSphere.ActivitySystem.ActivityClient
{
    public class NetHelper
    {
        #region Public Members
        /// <summary>
        /// Finds an available port by scanning all ports
        /// </summary>
        /// <returns>A valid port</returns>
        public static int FindPort()
        {
            int port = NO_PORT;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
                port = local.Port;
            }

            if (port == NO_PORT)
                throw new InvalidOperationException("The client was unable to find a free port.");

            return port;
        }

        /// <summary>
        /// Finds a valid IP address by scanning the network devices
        /// </summary>
        /// <param name="local">Indic</param>
        /// <returns></returns>
        public static string GetIP(IPType type)
        {
            string localIP = NO_IP;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (type == IPType.Local)
                    {
                        if(IsLocalIpAddress(ip.ToString()))
                        {
                            localIP = ip.ToString();
                            return localIP;
                        }
                    }
                    else
                        localIP = ip.ToString();
                }
            }

            if (localIP == NO_IP)
                throw new InvalidOperationException("The client was unable to detect an IP address or there is no active connection.");

            return localIP;
        }

        /// <summary>
        /// Checks if an IP address is local
        /// </summary>
        /// <param name="host">The IP address</param>
        /// <returns>A bool indicating if the IP address if local or not</returns>
        public static bool IsLocalIpAddress(string host)
        {
            try
            { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
        #endregion

        #region Constants
        public static string NO_IP = "NULL";
        public static int NO_PORT = -1;
        #endregion
    }

    public enum IPType
    {
        Local,
        All
    }
}
