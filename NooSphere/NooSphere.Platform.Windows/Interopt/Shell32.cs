using System;
using System.Runtime.InteropServices;

namespace NooSphere.Platform.Windows.Interopt
{
    public class Shell32
    {
        #region Constants
        const string shell32 = "shell32.dll";
        #endregion

        #region Shell32.Calls
        [DllImport(shell32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RegisterShellHook(IntPtr hwnd, int flags);

        [DllImport(shell32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);

        [DllImport(shell32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        #endregion

    }
}
