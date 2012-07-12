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
    public enum ShellFlags : int
    {
        SHGFI_ICON = 0x100,
        SHGFI_DISPLAYNAME = 0x200,
        SHGFI_TYPENAME = 0x400,
        SHGFI_ATTRIBUTES = 0x800,
        SHGFI_ICONLOCATION = 0x1000,
        SHGFI_EXETYPE = 0x2000,
        SHGFI_SYSICONINDEX = 0x4000,
        SHGFI_LINKOVERLAY = 0x8000,
        SHGFI_SELECTED = 0x10000,
        SHGFI_ATTR_SPECIFIED = 0x20000,
        SHGFI_LARGEICON = 0x0,
        SHGFI_SMALLICON = 0x1,
        SHGFI_OPENICON = 0x2,
        SHGFI_SHELLICONSIZE = 0x4,
        SHGFI_PIDL = 0x8,
        SHGFI_USEFILEATTRIBUTES = 0x10,
        SHGFI_ADDOVERLAYS = 0x20,
        SHGFI_OVERLAYINDEX = 0x40
    }
}
