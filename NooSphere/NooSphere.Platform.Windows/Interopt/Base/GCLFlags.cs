using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Platform.Windows.Interopt
{
    public enum GCLFlags : int
    {
        GCL_CBCLSEXTRA = -20,
        GCL_CBWNDEXTRA = -18,
        GCL_HBRBACKGROUND = -10,
        GCL_HCURSOR = -12,
        GCL_HICON = -14,
        GCL_HMODULE = -16,
        GCL_MENUNAME = -8,
        GCL_STYLE = -26,
        GCL_WNDPROC = -24,
        GCL_HICONSM = -34
    }
}
