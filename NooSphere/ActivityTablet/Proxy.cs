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
using NooSphere.Core.ActivityModel;
using System.Windows.Controls;

namespace ActivityTablet
{
    public class Proxy
    {
        public Activity Activity{get;set;}
        public Object Ui { get; set; }
        public System.Windows.Controls.Image Image
        {
            get;
            set;
        }
    }
}
