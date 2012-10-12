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
    public class User32
    {
        #region Constants
        const string user32 = "user32.dll";
        #endregion

        #region User32.Calls

        //[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern int SendMessageAPI(IntPtr hwnd, int msg, int wparam, int lparam);
        //[DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr FindWindowAPI(string className, string windowTitle);
        //[DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Ansi, SetLastError = true)]
        //public static extern IntPtr GetDesktopWindowAPI();
        //[DllImport("user32.dll", EntryPoint = "EndTask", CharSet = CharSet.Ansi, SetLastError = true)]
        //public static extern bool EndTaskAPI(IntPtr hwnd, bool shutdown, bool force);
        //[DllImport("user32.dll", EntryPoint = "GetParent", CharSet = CharSet.Ansi, SetLastError = true)]
        //public static extern IntPtr GetParentAPI(IntPtr hwnd);
        //[DllImport("user32.dll", EntryPoint = "GetForegroundWindow", CharSet = CharSet.Ansi, SetLastError = true)]
        //public static extern IntPtr GetForegroundWindowAPI();
        //[DllImport("user32.dll", EntryPoint = "IsChild", CharSet = CharSet.Ansi, SetLastError = true)]
        //public static extern bool IsChildAPI(IntPtr hwndParent, IntPtr hwnd);
        //[DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern int PostMessageAPI(IntPtr hwnd, int msg, int wparam, int lparam);
        //[DllImport("user32.dll", EntryPoint = "SendMessageTimeout", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern int SendMessageTimeoutAPI(IntPtr hwnd, int msg, int wparam, int lparam, int uflags, int uTimeout, ref int returnVal);
        //---

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hwnd, APIPOINT p, int flags);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeregisterShellHookWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int EnableWindow(IntPtr hwnd, Boolean fEnable);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EndTask(IntPtr hwnd, bool shutdown, bool force);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool EnumWindows(ListWindowDelegate callback, int lparam);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr hwnd, ListWindowDelegate callback, int lparam);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwnd, IntPtr afterChild, string className, string text);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, int flags);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hwnd, System.Text.StringBuilder className, int buffersize);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)] 
        public static extern IntPtr GetDesktopWindow();

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowModuleFileName(IntPtr hwnd, ref System.Text.StringBuilder lpszFileName, int cchFileNameMax);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int GetWindowRgn(IntPtr hwnd, ref IntPtr rgn);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder text, int maxLength);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hwnd, int wCmd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WININFO info);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, ref int procId);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsChild(IntPtr hwndParent, IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsHungAppWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsIconic(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsZoomed(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool refresh);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, int nflags);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int PostMessage(IntPtr hwnd, int msg, int wparam, int lparam);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint acc, uint keys);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RegisterShellHookWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, int msg, int wparam, int lparam);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessageTimeout(IntPtr hwnd, int msg, int wparam, int lparam, int uflags, int uTimeout, ref int returnVal);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetProgmanWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetTaskmanWindow(IntPtr hwnd);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int SetWindowRgn(IntPtr hwnd, IntPtr rgn, bool redraw);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hwnd, string newText);

        [DllImport(user32, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hInstance, int threadId);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SwitchToThisWindow(IntPtr hwnd, bool fAltTab);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SystemParametersInfo(Int32 uiAction, Int32 uiParam, String pvParam, Int32 WinIni);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr WindowFromPoint(APIPOINT p);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr idHook);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport(user32, CharSet = CharSet.Auto, SetLastError = true)]
	    public static extern bool UpdateWindow(IntPtr hwnd);

        #endregion
    }
}
