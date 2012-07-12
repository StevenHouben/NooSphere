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
    public class GDI32
    {
        #region Constants
        private const string gdi32 = "gdi32.dll";
        public  const int SRCCOPY = 0x00CC0020;
        #endregion

        #region GDI32.Calls
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool BitBlt(IntPtr hObject,int nXDest,int nYDest,int nWidth,int nHeight,IntPtr hObjectSource,int nXSrc,int nYSrc,int dwRop);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC,int nWidth, int nHeight);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hDC,IntPtr hObject);
        [DllImport(gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ReleaseDC(IntPtr dc);
        #endregion
    }
}
