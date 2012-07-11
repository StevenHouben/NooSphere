using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Discovery
{
    public class DiscoveryEventArgs:EventArgs
    {
        public List<string> ServicesAddresses { get; set; }
        public DiscoveryEventArgs() 
        {
            ServicesAddresses = new List<string>();
        }
        public DiscoveryEventArgs(List<string> addrs)
        {
            ServicesAddresses = new List<string>();
            this.ServicesAddresses = addrs;
        }
    }
}
