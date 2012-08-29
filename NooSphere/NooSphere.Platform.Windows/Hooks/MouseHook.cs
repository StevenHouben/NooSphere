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

using System.Runtime.InteropServices;
using System.Windows.Forms;
using NooSphere.Platform.Windows.Interopt;

namespace NooSphere.Platform.Windows.Hooks
{
    
    public class MouseHook
    {
        #region Private Members
        private static IntPtr hHook;
        private static HookEvents.HookProc hookProcedure;
        protected static bool IsRegistered = false;
        #endregion

        #region Events
        public static event MouseEventHandler MouseMove = null;
        public static event MouseEventHandler MouseDown = null;
        public static event MouseEventHandler MouseUp = null;
        #endregion

        #region Private Methods
        private static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var myMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode < 0)
                return User32.CallNextHookEx(hHook, nCode, wParam, lParam);
            HandleEvents(wParam, myMouseHookStruct);
            return User32.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        private static void HandleEvents(IntPtr wParam, MouseHookStruct mouse)
        {
            switch((WindowMessage)wParam)
            {
                case WindowMessage.WM_MOUSEMOVE:
                    MouseMove(new object(), new MouseEventArgs(MouseButtons.None, mouse.wHitTestCode, mouse.pt.x, mouse.pt.y, -1));
                    break;
                case WindowMessage.WM_LBUTTONDOWN:
                    MouseDown(new object(),new MouseEventArgs(MouseButtons.Left,0, mouse.pt.x, mouse.pt.y, -1));
                    break;
                case WindowMessage.WM_RBUTTONDOWN:
                    MouseDown(new object(), new MouseEventArgs(MouseButtons.Right, 0, mouse.pt.x, mouse.pt.y, -1));
                    break;
                case WindowMessage.WM_LBUTTONUP:
                    MouseUp(new object(), new MouseEventArgs(MouseButtons.Left, 0, mouse.pt.x, mouse.pt.y, -1));
                    break;
                case WindowMessage.WM_RBUTTONUP:
                    MouseUp(new object(), new MouseEventArgs(MouseButtons.Right, 0, mouse.pt.x, mouse.pt.y, -1));
                    break;
            }
        }
        #endregion

        #region Public Methods
        public static void Register()
        {
            if (hHook == IntPtr.Zero)
            {
                hookProcedure = new NooSphere.Platform.Windows.Hooks.HookEvents.HookProc(MouseHookProc);

                hHook = User32.SetWindowsHookEx((int)HookType.WH_MOUSE_LL,hookProcedure,(IntPtr)0,0);
                if (hHook == IntPtr.Zero)
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                IsRegistered = true;
            }
        }
        public static void UnRegister()
        {
            if (hHook != IntPtr.Zero)
            {
                bool ret = User32.UnhookWindowsHookEx(hHook);
                if (ret == false)
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                IsRegistered = false;
            } 
        }
        #endregion
    }
}
