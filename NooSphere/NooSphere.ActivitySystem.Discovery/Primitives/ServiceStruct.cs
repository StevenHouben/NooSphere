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

namespace NooSphere.ActivitySystem.Discovery.Primitives
{
    public class ServiceStruct
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public ServiceStruct() { }
        public ServiceStruct(string name,string location, string addr)
        {
            this.Name = name;
            this.Location = location;
            this.Address = addr;
        }
    }
}
