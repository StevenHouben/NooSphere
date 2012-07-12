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
using NooSphere.Core.Primitives;
using System.IO;

namespace NooSphere.Core.ActivityModel
{
    public class Resource : Identity
    {
        public Resource()
            : base()
        {
        }

        public Guid ActivityId { get; set; }
        public Guid ActionId { get; set; }
        public int Size { get; set; }
        public string CreationTime { get; set; }
        public string LastWriteTime { get; set; }
        public string RelativePath { get; set; }
        public Service Service { get; set; }
    }
}
