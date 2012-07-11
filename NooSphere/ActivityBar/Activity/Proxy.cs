using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Platform.Windows.VDM;
using NooSphere.Core.Primitives;
using NooSphere.Core.ActivityModel;

namespace ActivityUI
{
    public class Proxy:Identity
    {
     
        public VirtualDesktop Desktop { get; set; }
        public Activity Activity{get;set;}
        public System.Windows.Controls.Image Image
        {
            get;
            set;
        }
    }
}
