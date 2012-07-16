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
using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.ActivityManager
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
