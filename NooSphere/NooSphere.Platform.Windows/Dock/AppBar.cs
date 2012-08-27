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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using NooSphere.Platform.Windows.Interopt;

namespace NooSphere.Platform.Windows.Dock
{
    public static class AppBarFunctions
    {
        private class RegisterInfo
        {
            public int CallbackId { get; set; }
            public bool IsRegistered { get; set; }
            public Window Window { get; set; }
            public AppBarPosition Edge { get; set; }
            public WindowStyle OriginalStyle { get; set; }
            public Point OriginalPosition { get; set; }
            public Size OriginalSize { get; set; }
            public ResizeMode OriginalResizeMode { get; set; }
            public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,IntPtr lParam, ref bool handled)
            {
                if (msg == CallbackId)
                {
                    if (wParam.ToInt32() == (int)ABNotify.ABN_POSCHANGED)
                    {
                        ABSetPos(Edge, Window);
                        handled = true;
                    }
                }
                return IntPtr.Zero;
            }

        }
        private static Dictionary<Window, RegisterInfo> registeredWindowInfo= new Dictionary<Window, RegisterInfo>();
        private static RegisterInfo GetRegisterInfo(Window appbarWindow)
        {
            RegisterInfo reg;
            if (registeredWindowInfo.ContainsKey(appbarWindow))
            {
                reg = registeredWindowInfo[appbarWindow];
            }
            else
            {
                reg = new RegisterInfo()
                {
                    CallbackId = 0,
                    Window = appbarWindow,
                    IsRegistered = false,
                    Edge = AppBarPosition.Top,
                    OriginalStyle = appbarWindow.WindowStyle,
                    OriginalPosition = new Point(appbarWindow.Left, appbarWindow.Top),
                    OriginalSize = new Size(appbarWindow.ActualWidth, appbarWindow.ActualHeight),
                    OriginalResizeMode = appbarWindow.ResizeMode,
                };
                registeredWindowInfo.Add(appbarWindow, reg);
            }
            return reg;
        }
        private static void RestoreWindow(Window appbarWindow)
        {
            RegisterInfo info = GetRegisterInfo(appbarWindow);

            appbarWindow.WindowStyle = info.OriginalStyle;
            appbarWindow.ResizeMode = info.OriginalResizeMode;

            Rect rect = new Rect(info.OriginalPosition.X, info.OriginalPosition.Y,
                info.OriginalSize.Width, info.OriginalSize.Height);
            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new ResizeDelegate(DoResize), appbarWindow, rect);

        }
        private delegate void ResizeDelegate(Window appbarWindow, Rect rect);
        private static void DoResize(Window appbarWindow, Rect rect)
        {
            appbarWindow.Width = rect.Width;
            appbarWindow.Height = rect.Height;
            appbarWindow.Top = rect.Top;
            appbarWindow.Left = rect.Left;
        }
        private static void ABSetPos(AppBarPosition edge, Window appbarWindow)
        {
            APPBARDATA barData = new APPBARDATA();
            barData.cbSize = Marshal.SizeOf(barData);
            barData.hWnd = new WindowInteropHelper(appbarWindow).Handle;
            barData.uEdge = (int)edge;

            if (barData.uEdge == (int)AppBarPosition.Left || barData.uEdge == (int)AppBarPosition.Right)
            {
                barData.rc.top = 0;
                barData.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                if (barData.uEdge == (int)AppBarPosition.Left)
                {
                    barData.rc.left = 0;
                    barData.rc.right = (int)Math.Round(appbarWindow.ActualWidth);
                }
                else
                {
                    barData.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                    barData.rc.left = barData.rc.right - (int)Math.Round(appbarWindow.ActualWidth);
                }
            }
            else
            {
                barData.rc.left = 0;
                barData.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                if (barData.uEdge == (int)AppBarPosition.Top)
                {
                    barData.rc.top = 0;
                    barData.rc.bottom = (int)Math.Round(appbarWindow.ActualHeight);
                }
                else
                {
                    barData.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                    barData.rc.top = barData.rc.bottom - (int)Math.Round(appbarWindow.ActualHeight);
                }
            }

            Shell32.SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref barData);
            Shell32.SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref barData);

            Rect rect = new Rect((double)barData.rc.left, (double)barData.rc.top,
                (double)(barData.rc.right - barData.rc.left), (double)(barData.rc.bottom - barData.rc.top));
            //This is done async, because WPF will send a resize after a new appbar is added.  
            //if we size right away, WPFs resize comes last and overrides us.
            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new ResizeDelegate(DoResize), appbarWindow, rect);
        }

        #region Public Methods
        public static void SetAppBar(Window appbarWindow, AppBarPosition edge)
        {
            RegisterInfo info = GetRegisterInfo(appbarWindow);
            info.Edge = edge;

            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = new WindowInteropHelper(appbarWindow).Handle;

            if (edge == AppBarPosition.None)
            {
                if (info.IsRegistered)
                {
                    Shell32.SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                    info.IsRegistered = false;
                }
                RestoreWindow(appbarWindow);
                return;
            }

            if (!info.IsRegistered)
            {
                info.IsRegistered = true;
                info.CallbackId = User32.RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = info.CallbackId;

                uint ret = Shell32.SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);

                HwndSource source = HwndSource.FromHwnd(abd.hWnd);
                source.AddHook(new HwndSourceHook(info.WndProc));
            }

            appbarWindow.WindowStyle = WindowStyle.None;
            appbarWindow.ResizeMode = ResizeMode.NoResize;
            appbarWindow.Topmost = true;

            ABSetPos(info.Edge, appbarWindow);
        }
        #endregion
    }
}