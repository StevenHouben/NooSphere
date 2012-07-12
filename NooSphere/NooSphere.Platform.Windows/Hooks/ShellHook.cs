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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

using NooSphere.Platform.Windows.Interopt;
using NooSphere.Platform.Windows.Win32.Shell;
using NooSphere.Platform.Windows.Windowing;
using NooSphere.Platform.Windows.InteroptServices;

namespace NooSphere.Platform.Windows.Hooks
{
    public class ShellHook
    {
        public event ShellEvents.WindowCreatedEventHandler WindowCreated;
        public event ShellEvents.WindowDestroyedEventHandler WindowDestroyed;
        public event ShellEvents.WindowActivatedEventHandler WindowActivated;
        public event ShellEvents.WindowReplacedEventHandler WindowReplaced;
        public event ShellEvents.ActivateShellWindowEventHandler ActivateShellWindow;
        public event ShellEvents.WindowTitleChangeEventHandler WindowTitleChange;
        public event ShellEvents.OpenTaskManagerEventHandler OpenTaskManager;
        public event ShellEvents.ApplicationCommandEventHandler ApplicationCommand;
        public event ShellEvents.GetMinimizedRectEventHandler GetMinimizedRect;

        private const int RSH_UNREGISTER = 0;
        private const int RSH_REGISTER = 1;
        private const int RSH_REGISTER_PROGMAN = 2;
        private const int RSH_REGISTER_TASKMAN = 3;
        private const int WM_COPYDATA = 0x4a;
        
        static int WM_SHELLHOOKMESSAGE;
        static NativeWindowEx hookWin;
        static IntPtr replacingWindow;

        public void InitShellEvents()
        {
            try
            {
                hookWin = new NativeWindowEx();
                hookWin.CreateHandle(new CreateParams());

                //Register to receive shell-related events
                if (User32.RegisterShellHookWindow(hookWin.Handle) == false)
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                else
                {
                    //No error occurred
                    WM_SHELLHOOKMESSAGE = User32.RegisterWindowMessage("SHELLHOOK");
                    hookWin.MessageRecieved += ShellWinProc;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw ex;
            }
        }
        public void UninitShellEvents()
        {
            Shell32.RegisterShellHook(hookWin.Handle, RSH_UNREGISTER);
        }
        private void ShellWinProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_SHELLHOOKMESSAGE)
                {
                    switch ((ShellMessages)m.WParam)
                    {
                        case ShellMessages.HSHELL_WINDOWCREATED:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowCreated != null)
                                {
                                    WindowCreated(new WindowInfo(m.LParam));
                                    Console.WriteLine(m.LParam.ToString());
                                }

                            break;
                        case ShellMessages.HSHELL_WINDOWDESTROYED:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowDestroyed != null)
                                {
                                    WindowDestroyed(m.LParam);
                                }

                            break;
                        case ShellMessages.HSHELL_WINDOWREPLACING:
                            replacingWindow = m.LParam;
                            break;
                        case ShellMessages.HSHELL_WINDOWREPLACED:
                            if (WindowManager.IsValid(replacingWindow) && WindowManager.IsValid(m.LParam))
                                if (WindowReplaced != null)
                                {
                                    WindowReplaced(new WindowInfo(replacingWindow), new WindowInfo(m.LParam));
                                }

                            break;
                        case ShellMessages.HSHELL_WINDOWACTIVATED:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowActivated != null)
                                {
                                    WindowActivated(new WindowInfo(m.LParam), false);
                                }

                            break;
                        case ShellMessages.HSHELL_FLASH:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowTitleChange != null)
                                {
                                    WindowTitleChange(new WindowInfo(m.LParam), true);
                                }

                            break;
                        case ShellMessages.HSHELL_RUDEAPPACTIVATED:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowActivated != null)
                                {
                                    WindowActivated(new WindowInfo(m.LParam), false);
                                }

                            break;
                        case ShellMessages.HSHELL_REDRAW:
                            if (WindowManager.IsValid(m.LParam))
                                if (WindowTitleChange != null)
                                {
                                    WindowTitleChange(new WindowInfo(m.LParam), false);
                                }

                            break;
                        case ShellMessages.HSHELL_APPCOMMAND:
                            bool cancel = false;
                            if (ApplicationCommand != null)
                            {
                                ApplicationCommand(null, (ApplicationCommandType)(Convert.ToInt32(m.LParam) >> 8), ref cancel);
                            }
                            break;
                        case ShellMessages.HSHELL_ACTIVATESHELLWINDOW:
                            if (ActivateShellWindow != null)
                            {
                                ActivateShellWindow();
                            }

                            break;
                        case ShellMessages.HSHELL_TASKMAN:
                            cancel = false;
                            if (OpenTaskManager != null)
                            {
                                OpenTaskManager(ref cancel);
                            }

                            break;
                        case ShellMessages.HSHELL_GETMINRECT:
                            SHELLHOOKINFO winHandle = (SHELLHOOKINFO)Marshal.PtrToStructure(m.LParam, typeof(SHELLHOOKINFO));
                            //Dim ptr As IntPtr = Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf(GetType(RECT)))
                            //Marshal.StructureToPtr(winHandle.rc, ptr, True)
                            //m.Result = ShellProc(HSHELL_GETMINRECT, winHandle.hwnd, ptr)
                            //winHandle.rc = Marshal.PtrToStructure(ptr, GetType(RECT))
                            //winHandle.rc = New RECT
                            WindowInfo wi = new WindowInfo(winHandle.hwnd);
                            winHandle.rc.top = 0;
                            winHandle.rc.left = 0;
                            winHandle.rc.right = 100;
                            winHandle.rc.bottom = 100;
                            Marshal.StructureToPtr(winHandle, m.LParam, true);
                            m.Result = winHandle.hwnd;
                            break;
                        //Marshal.FreeHGlobal(ptr)
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("An error occured in ShellWinProc: " + ex.Message);
            }
        }
    }
}
