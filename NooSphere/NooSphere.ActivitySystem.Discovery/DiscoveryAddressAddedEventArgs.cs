using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Discovery
{
    public class DiscoveryAddressAddedEventArgs:EventArgs
    {
        public ServicePair ServicePair{ get; set; }
        public DiscoveryAddressAddedEventArgs() { }
        public DiscoveryAddressAddedEventArgs(ServicePair pair)
        {
            ServicePair = new ServicePair();
            this.ServicePair = pair;
        }
    }
}
