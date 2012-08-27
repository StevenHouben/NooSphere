/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.Base.Service
{
    public class ConnectedClient
    {
        public string Name { get; private set; }
        public string Ip { get; private set; }
        public Device Device { get; set; }
        public ConnectedClient(string name, string ip, Device devi) 
        {
            Name = name;
            Ip = ip;
            Device = devi; 
        }
        public override string ToString()
        {
            return Ip;
        }
    }
}
