using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
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
            int flags = 0;
            User32.SHSetKnownFolderPath(ref KnownFolder.Desktop, (uint)flags, IntPtr.Zero, path);
            User32.SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
            return true;
        }
    }
}
