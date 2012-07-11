using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Discovery
{
    public class ServicePair
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public ServicePair() { }
        public ServicePair(string name, string addr)
        {
            this.Name = name;
            this.Address = addr;
        }
    }
}
