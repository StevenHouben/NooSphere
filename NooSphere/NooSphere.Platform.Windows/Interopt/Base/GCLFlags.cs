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
