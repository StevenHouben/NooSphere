using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NooSphere.MonoDroid.ActivitySystem.Discovery
{
    public class MonoDroidWSDiscovery
    {
        #region Private Members
        private const int WsDiscoveryPort = 3702;
        private readonly UdpClient _udpClient;
        #endregion

        #region Constructor
        public MonoDroidWSDiscovery()
        {
            _udpClient = new UdpClient(WsDiscoveryPort);
        }
        #endregion

        #region HttpHandlers
        private void HandleRequest(IAsyncResult result)
        {
        }
        #endregion
    }
}