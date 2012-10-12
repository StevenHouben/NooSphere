/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using NooSphere.Platform.Windows.Interopt;
namespace NooSphere.Platform.Windows.VDM
{
    public class DesktopFolderSwitcher
    {
        public static class KnownFolder
        {
            public static Guid Desktop = new Guid("B4BFCC3A-DB2C-424C-B029-7FE99A87C641");
        }
        public static bool ChangeDesktopFolder(string path)
        {
            var flags = 0;
            Shell32.SHSetKnownFolderPath(ref KnownFolder.Desktop, (uint)flags, IntPtr.Zero, path);
            Shell32.SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
            return true;
        }
    }
}
