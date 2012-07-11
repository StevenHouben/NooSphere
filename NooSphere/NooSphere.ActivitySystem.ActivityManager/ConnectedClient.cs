using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.ActivityService
{
    public class ConnectedClient
    {
        public string Name { get; private set; }
        public string IP { get; private set; }
        public Device Device { get; set; }
        public ConnectedClient(string name, string ip, Device devi) 
        {
            this.Name = name;
            this.IP = ip;
            this.Device = devi; 
        }
        public override string ToString()
        {
            return IP;
        }
    }
}
