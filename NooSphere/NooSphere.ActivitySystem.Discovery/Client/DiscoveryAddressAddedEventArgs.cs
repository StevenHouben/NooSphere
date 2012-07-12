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
using NooSphere.ActivitySystem.Discovery.Primitives;

namespace NooSphere.ActivitySystem.Discovery.Client
{
    public class DiscoveryAddressAddedEventArgs:EventArgs
    {
        public ServiceStruct ServicePair{ get; set; }
        public DiscoveryAddressAddedEventArgs() { }
        public DiscoveryAddressAddedEventArgs(ServiceStruct pair)
        {
            ServicePair = new ServiceStruct();
            this.ServicePair = pair;
        }
    }
}
