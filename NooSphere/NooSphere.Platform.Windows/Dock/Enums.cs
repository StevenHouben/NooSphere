using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Platform.Windows.Dock
{
    public enum AppBarPosition : int
    {
        Left = 0,
        Top,
        Right,
        Bottom,
        None
    }
    internal enum ABMsg : int
    {
        ABM_NEW = 0,
        ABM_REMOVE,
        ABM_QUERYPOS,
        ABM_SETPOS,
        ABM_GETSTATE,
        ABM_GETTASKBARPOS,
        ABM_ACTIVATE,
        ABM_GETAUTOHIDEBAR,
        ABM_SETAUTOHIDEBAR,
        ABM_WINDOWPOSCHANGED,
        ABM_SETSTATE
    }
    internal enum ABNotify : int
    {
        ABN_STATECHANGE = 0,
        ABN_POSCHANGED,
        ABN_FULLSCREENAPP,
        ABN_WINDOWARRANGE
    }
}
