using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NooSphere.Platform.Windows.Windowing;
using NooSphere.Platform.Windows.Windowing;

namespace NooSphere.Platform.Windows.VDM
{
    public class WindowDescription
    {
        public Rectangle Rectangle { get; set; }
        public WindowInfo.WindowState State { get; set; }
        public string Name { get; set; }
        public string Application { get; set; }
        public string Content { get; set; }
    }
}
