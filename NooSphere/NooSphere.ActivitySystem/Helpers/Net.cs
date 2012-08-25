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
using System.Net;
using System.Net.Sockets;

namespace NooSphere.ActivitySystem.Helpers
{
    public static class Net
    {
        #region Public Members
        /// <summary>
        /// Finds an available port by scanning all ports
        /// </summary>
        /// <returns>A valid port</returns>
        public static int FindPort()
        {
            var port = NoPort;

            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                var local = (IPEndPoint)socket.LocalEndPoint;
                port = local.Port;
            }

            if (port == NoPort)
                throw new InvalidOperationException("The client was unable to find a free port.");

            return port;
        }

        /// <summary>
        /// Finds a valid IP address by scanning the network devices
        /// </summary>
        /// <returns></returns>
        public static string GetIp(IPType type)
        {
            var localIp = NoIP;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (type == IPType.Local)
                    {
                        if (IsLocalIpAddress(ip.ToString()))
                        {
                            localIp = ip.ToString();
                            return localIp;
                        }
                    }
                    else
                        localIp = ip.ToString();
                }
            }

            if (localIp == NoIP)
                throw new InvalidOperationException("The client was unable to detect an IP address or there is no active connection.");

            return localIp;
        }

        /// <summary>
        /// Checks if an IP address is local
        /// </summary>
        /// <param name="host">The IP address</param>
        /// <returns>A bool indicating if the IP address if local or not</returns>
        public static bool IsLocalIpAddress(string host)
        {
            var hostIPs = Dns.GetHostAddresses(host);
            // get local IP addresses
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            // test if any host IP equals to any local IP or to localhost
            foreach (var hostIP in hostIPs)
            {
                // is localhost
                if (IPAddress.IsLoopback(hostIP)) return true;
                // is local address
                foreach (IPAddress localIP in localIPs)
                {
                    if (hostIP.Equals(localIP)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Constructs an url based on given parameters
        /// </summary>
        /// <param name="ip">Base ip address</param>
        /// <param name="port">Base port</param>
        /// <param name="relative">Relate path</param>
        /// <returns></returns>
        public static Uri GetUrl(string ip, int port, string relative)
        {
            return new Uri(string.Format("http://{0}:{1}/{2}", ip, port, relative));
        }
        #endregion

        #region Constants
        public static string NoIP = "NULL";
        public static int NoPort = -1;
        #endregion
    }

    public enum IPType
    {
        Local,
        All
    }
}
