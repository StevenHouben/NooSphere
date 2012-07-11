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
