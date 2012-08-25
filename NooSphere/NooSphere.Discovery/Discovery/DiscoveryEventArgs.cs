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
using System.Collections.Generic;

namespace NooSphere.Discovery.Discovery
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
            ServicesAddresses = addrs;
        }
    }
}
