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
    public enum ShellMessages : int
    {
        HSHELL_WINDOWCREATED = 1,
        HSHELL_WINDOWDESTROYED = 2,
        HSHELL_ACTIVATESHELLWINDOW = 3,
        HSHELL_WINDOWACTIVATED = 4,
        HSHELL_GETMINRECT = 5,
        HSHELL_REDRAW = 6,
        HSHELL_TASKMAN = 7,
        HSHELL_LANGUAGE = 8,
        HSHELL_SYSMENU = 9,
        HSHELL_ENDTASK = 10,
        HSHELL_ACCESSIBILITYSTATE = 11,
        HSHELL_APPCOMMAND = 12,
        HSHELL_WINDOWREPLACED = 13,
        HSHELL_WINDOWREPLACING = 14,
        HSHELL_HIGHBIT = 0x8000,
        HSHELL_FLASH = (HSHELL_REDRAW | HSHELL_HIGHBIT),
        HSHELL_RUDEAPPACTIVATED = (HSHELL_WINDOWACTIVATED | HSHELL_HIGHBIT)
    }
}
