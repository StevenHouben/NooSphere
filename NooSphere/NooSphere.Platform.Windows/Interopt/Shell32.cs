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

        [DllImport(shell32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        [DllImport(shell32, CharSet = CharSet.Auto, SetLastError = true)]
        public extern static Int32 SHSetKnownFolderPath(ref Guid folderId, uint flags, IntPtr token, [MarshalAs(UnmanagedType.LPWStr)] string path);
        #endregion

    }
}
