using System;
using System.Runtime.InteropServices;

namespace NooSphere.Platform.Windows.Interopt
{
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public int hIcon;
            public int iIcon;
            public int dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLHOOKINFO
        {
            public IntPtr hwnd;
            public RECT rc;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public APIPOINT ptReserved;
            public APIPOINT ptMaxSize;
            public APIPOINT ptMaxPosition;
            public APIPOINT ptMinTrackSize;
            public APIPOINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APIPOINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public int dwDataPtr;
            public int cbData;
            public int lpData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int Length;
            public int flags;
            public int showCmd;
            public APIPOINT ptMinPosition;
            public APIPOINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WININFO
        {
            public int cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public int dwStyle;
            public int dwExStyle;
            public int dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public short atomWindowType;
            public short wCreatorVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
}
