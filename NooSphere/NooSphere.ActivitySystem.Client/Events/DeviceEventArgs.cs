using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.Client.Events
{
    public class DeviceEventArgs
    {
        public Device Device { get; set; }
        public DeviceEventArgs() { }
        public DeviceEventArgs(Device device)
        {
            this.Device = device;
        }
    }
}
