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
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Microsoft.VisualBasic;
using System.Drawing.Imaging;
using NooSphere.Platform.Windows.Interopt;
using System.Collections.ObjectModel;

namespace NooSphere.Platform.Windows.Windowing
{
    public class WindowInfo
    {
        private IntPtr hWnd = IntPtr.Zero;
        private Collection<WindowInfo> children = new Collection<WindowInfo>();
        private WindowChooseDelegate childChooser;

        public WindowInfo(IntPtr handle)
        {
            if (User32.IsWindow(handle))
                hWnd = handle;
            else
                throw new Exception("The specified window handle does not represent any existing windows.");
        }

        public string ClassName
        {
            get
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder(256);
                if (User32.GetClassName(hWnd, s, s.Capacity) == 0)
                {
                    throw new Exception("Error retrieving window class name.");
                }
                return s.ToString();
            }
        }
        public string Fullname
        {
            get
            {
                System.Text.StringBuilder modulePathname = new System.Text.StringBuilder(1024);
                int length = User32.GetWindowModuleFileName(hWnd, ref modulePathname, modulePathname.Capacity);
                return modulePathname.ToString(0, length);
            }
        }
        public bool ShowInTaskbar
        {
            get
            {
                if ((this.Visible) & (User32.GetParent(hWnd).ToInt32() == 0))
                {
                    // extended options call up
                    int exStyles = (int)User32.GetWindowLong(hWnd, GWL_EXSTYLE);
                    // parents windows examine again:
                    IntPtr ownerWin = User32.GetWindow(hWnd, GW_OWNER);
                    // examining whether:
                    // - no ToolWindow and no Childfenster or
                    // - application windows and Childfenster
                    if ((((exStyles & (int)ExtendedWindowStyle.WS_EX_TOOLWINDOW) == 0) & (ownerWin.ToInt32() == 0)) | ((exStyles & (int)ExtendedWindowStyle.WS_EX_APPWINDOW) == 0) & (ownerWin.ToInt32() != 0))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public WindowIconSize IconSize { get; set; }
        public Icon Icon
        {
            get
            {
                try
                {
                    IntPtr hIco = new IntPtr(SendMessageTimeout((int)WindowMessage.WM_GETICON, Convert.ToInt32(IconSize), 0, SendMessageTimeoutFlags.AbortIfHung, 200));
                    if (hIco.ToInt32() == 0)
                    {
                        hIco = new IntPtr(User32.GetClassLong(hWnd, (int)(IconSize == WindowIconSize.Big ? (int)GCLFlags.GCL_HICON : (int)GCLFlags.GCL_HICONSM)));
                        if (hIco.ToInt32() == 0)
                        {
                            SHFILEINFO shfi = new SHFILEINFO();
                            string fileName = this.Process.MainModule.FileName;
                            if (!(fileName.Length > 260))
                            {
                                Shell32.SHGetFileInfo(fileName, 0, ref shfi, Marshal.SizeOf(shfi), (int)ShellFlags.SHGFI_ICON | (IconSize == WindowIconSize.Big ? (int)ShellFlags.SHGFI_LARGEICON : (int)ShellFlags.SHGFI_SMALLICON));
                                hIco = new IntPtr(shfi.hIcon);
                            }
                        }
                    }
                    return System.Drawing.Icon.FromHandle(hIco);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    return SystemIcons.Application;
                }
            }
            set { PostMessage((int)WindowMessage.WM_SETICON, Convert.ToInt32((IconSize == WindowIconSize.Small2 ? WindowIconSize.Small : IconSize)), value.Handle.ToInt32()); }
        }
        public string Text
        {
            get
            {
                int length = this.SendMessageTimeout((int)WindowMessage.WM_GETTEXTLENGTH, 0, 0, SendMessageTimeoutFlags.AbortIfHung, 1000);
                //GetWindowTextLength(hWnd) For some reason, this seems to just freeze
                if (length <= 0)
                    return string.Empty;
                string str = null;
                //The length should never be more than 300
                length = Math.Min(300, length);
                IntPtr ptr = System.Runtime.InteropServices.Marshal.StringToHGlobalAuto(new String(Convert.ToChar(0x0), length + 1));
                try
                {
                    if (this.SendMessageTimeout((int)WindowMessage.WM_GETTEXT, length + 1, ptr.ToInt32(), SendMessageTimeoutFlags.AbortIfHung, 500) <= 0)
                        return string.Empty;
                    str = System.Runtime.InteropServices.Marshal.PtrToStringAuto(ptr, length).Trim();
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
                }
                //GetWindowText(hWnd, t, length + 1) This seems to be freezing
                return str;
            }
            set
            {
                if (User32.SetWindowText(hWnd, value) == false)
                {
                    throw new Exception("An error occured setting the window text.");
                }
            }
        }
        public Region Region
        {
            get
            {
                IntPtr r = default(IntPtr);
                User32.GetWindowRgn(hWnd, ref r);
                return System.Drawing.Region.FromHrgn(r);
            }
            set { User32.SetWindowRgn(hWnd, value.GetHrgn(null), true); }
        }
        public Process Process
        {
            get { return System.Diagnostics.Process.GetProcessById(ProcessId); }
        }
        public int ProcessId
        {
            get
            {
                int procId = 0;
                int threadId = User32.GetWindowThreadProcessId(hWnd, ref procId);
                return procId;
            }
        }
        public ProcessThread Thread
        {
            get
            {
                int procId = 0;
                int threadId = User32.GetWindowThreadProcessId(hWnd, ref procId);
                return Process.GetProcessById(procId).Threads[threadId];
            }
        }
        public int ThreadId
        {
            get
            {
                int procId = 0;
                return User32.GetWindowThreadProcessId(hWnd, ref procId);
            }
        }
        public IntPtr Handle
        {
            get { return hWnd; }
        }
        public int SendMessage(ref Message m)
        {
            m.Result = new IntPtr(SendMessage(m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32()));
            return m.Result.ToInt32();
        }
        public int SendMessage(int msg, int wparam, int lparam)
        {
            return User32.SendMessage(hWnd, msg, wparam, lparam);
        }
        public int PostMessage(ref Message m)
        {
            m.Result = new IntPtr(User32.PostMessage(m.HWnd, m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32()));
            return m.Result.ToInt32();
        }
        public int PostMessage(int msg, int wparam, int lparam)
        {
            return User32.PostMessage(hWnd, msg, wparam, lparam);
        }
        public int SendMessageTimeout(ref Message m, SendMessageTimeoutFlags flags, int timeout)
        {
            m.Result = new IntPtr(SendMessageTimeout(m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32(), flags, timeout));
            return m.Result.ToInt32();
        }
        public int SendMessageTimeout(int msg, int wparam, int lparam, SendMessageTimeoutFlags flags, int timeout)
        {
            int result = 0;
            User32.SendMessageTimeout(hWnd, msg, wparam, lparam, (int)flags, timeout, ref result);
            return result;
        }
        public Rectangle Rectangle
        {
            get
            {
                RECT r = default(RECT);
                if (User32.GetWindowRect(hWnd, ref r) == false)
                {
                    throw new Exception("Error retrieving window rectangle.");
                }
                return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
            }
            set { Move(value, true); }
        }
        public int Width
        {
            get { return this.Rectangle.Width; }
            set { this.Rectangle = new Rectangle(this.Rectangle.X, this.Rectangle.Y, value, this.Rectangle.Height); }
        }
        public int Height
        {
            get { return this.Rectangle.Height; }
            set { this.Rectangle = new Rectangle(this.Rectangle.X, this.Rectangle.Y, this.Rectangle.Width, value); }
        }
        public Size Size
        {
            get { return this.Rectangle.Size; }
            set { this.Rectangle = new Rectangle(this.Rectangle.X, this.Rectangle.Y, value.Width, value.Height); }
        }
        public int Top
        {
            get { return this.Rectangle.Top; }
            set { this.Rectangle = new Rectangle(this.Rectangle.X, value, this.Rectangle.Width, this.Rectangle.Height); }
        }
        public int Left
        {
            get { return this.Rectangle.Left; }
            set { this.Rectangle = new Rectangle(value, this.Rectangle.Y, this.Rectangle.Width, this.Rectangle.Height); }
        }
        public Point Location
        {
            get { return this.Rectangle.Location; }
            set { this.Rectangle = new Rectangle(value.X, value.Y, this.Rectangle.Width, this.Rectangle.Height); }
        }
        public bool Visible
        {
            get { return User32.IsWindowVisible(hWnd); }
            set { State = (value ? WindowState.Show : WindowState.Hide); }
        }
        public bool Enabled
        {
            get { return User32.IsWindowEnabled(hWnd); }
            set { User32.EnableWindow(hWnd, value); }
        }
        public WindowState State
        {
            get
            {
                WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                wp.Length = Marshal.SizeOf(wp);
                User32.GetWindowPlacement(hWnd, ref wp);
                return (WindowState)wp.showCmd;
            }
            set { User32.ShowWindow(hWnd, (int)value); }
        }
        public Rectangle NormalRectangle
        {
            get
            {
                WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                wp.Length = Marshal.SizeOf(wp);
                User32.GetWindowPlacement(hWnd, ref wp);
                return Rectangle.FromLTRB(wp.rcNormalPosition.left, wp.rcNormalPosition.top, wp.rcNormalPosition.right, wp.rcNormalPosition.bottom);
            }
            set
            {
                WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                wp.Length = Marshal.SizeOf(wp);
                User32.GetWindowPlacement(hWnd, ref wp);
                RECT r = default(RECT);
                r.top = value.Top;
                r.bottom = value.Bottom;
                r.left = value.Left;
                r.right = value.Right;
                wp.rcNormalPosition = r;
                int state =
                wp.showCmd = (Visible ? (int)State : (int)WindowState.Hide);
                User32.SetWindowPlacement(hWnd, ref wp);
            }
        }
        public Point MaximizedLocation
        {
            get
            {
                WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                wp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                User32.GetWindowPlacement(hWnd, ref wp);
                return new Point(wp.ptMaxPosition.x, wp.ptMaxPosition.y);
            }
            set
            {
                WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                wp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                User32.GetWindowPlacement(hWnd, ref wp);
                APIPOINT p = default(APIPOINT);
                p.x = value.X;
                p.y = value.Y;
                wp.ptMaxPosition = p;
                wp.showCmd = (Visible ? (int)State : (int)WindowState.Hide);
                User32.SetWindowPlacement(hWnd, ref wp);
            }
        }
        public void Refresh()
        {
            if (User32.InvalidateRect(hWnd, IntPtr.Zero, true) == false)
                throw new System.ComponentModel.Win32Exception();
            if (User32.UpdateWindow(hWnd) == false)
                throw new Exception("Error updating the window!");
        }
        public void Move(Rectangle r,bool shouldRefresh)
        {
            if (User32.MoveWindow(hWnd, r.X, r.Y, r.Width, r.Height, shouldRefresh) == false)
                throw new Exception("Error moving the window.");
        }
        public WindowInfo ChildFromPoint(Point p)
        {
            APIPOINT pt = default(APIPOINT);
            pt.x = p.X;
            pt.y = p.Y;
            return new WindowInfo(User32.ChildWindowFromPointEx(hWnd, pt, (int)ChildFromPointOptions.All));
        }
        public WindowInfo ChildFromPoint(Point p, ChildFromPointOptions options)
        {
            APIPOINT pt = default(APIPOINT);
            pt.x = p.X;
            pt.y = p.Y;
            return new WindowInfo(User32.ChildWindowFromPointEx(hWnd, pt, (int)options));
        }
        public enum WindowRelation : int
        {
            Child = 5,
            FirstSibling = 0,
            LastSibling = 1,
            NextSibling = 2,
            PreviousSibling = 3,
            Owner = 4
        }
        public WindowInfo GetRelatedWindow(WindowRelation relation)
        {
            return new WindowInfo(User32.GetWindow(hWnd, (int)relation));
        }
        public bool NotResponding
        {
            get { return User32.IsHungAppWindow(hWnd); }
        }
        public void EndTask(bool force)
        {
            if (User32.EndTask(hWnd, false, force) == false)
                throw new Exception("Error ending the task.");
        }
        public Collection<WindowInfo> GetChildWindows()
        {
            return GetChildWindows(null);
        }
        public Collection<WindowInfo> GetChildWindows(WindowChooseDelegate selector)
        {
            children.Clear();
            childChooser = selector;
            if (User32.EnumChildWindows(hWnd, ListChildWindow, 0) == false)
                throw new Exception("Error enumerating child windows.");
            childChooser = null;
            return children;
        }
        private bool ListChildWindow(IntPtr hwnd, int lparam)
        {
            if (childChooser == null || childChooser(new WindowInfo(hwnd)))
            {
                children.Add(new WindowInfo(hwnd));
            }
            return true;
        }
        public WindowInfo FindChild(string className, string text)
        {
            return new WindowInfo(User32.FindWindowEx(hWnd, (IntPtr)0, className, text));
        }
        public WindowInfo FindChildByClass(string className)
        {
            return FindChild(className, null);
        }
        public WindowInfo FindChildByText(string text)
        {
            return FindChild(null, text);
        }
        public Collection<WindowInfo> FindChildren(string className, string text)
        {
            bool canContinue = true;
            System.Collections.ObjectModel.Collection<WindowInfo> children = new System.Collections.ObjectModel.Collection<WindowInfo>();
            do
            {
                IntPtr lastHandle = default(IntPtr);
                if (children.Count != 0)
                    lastHandle = children[children.Count - 1].Handle;
                IntPtr h = User32.FindWindowEx(hWnd, lastHandle, className, text);
                if (h.ToInt32() == 0)
                {
                    canContinue = false;
                }
                else
                {
                    children.Add(new WindowInfo(h));
                }
            } while (!(canContinue == false));
            return children;
        }
        public Collection<WindowInfo> FindChildrenByClass(string className)
        {
            return FindChildren(className, null);
        }
        public Collection<WindowInfo> FindChildrenByText(string text)
        {
            return FindChildren(null, text);
        }
        public WindowInfo GetRoot()
        {
            return new WindowInfo(User32.GetAncestor(hWnd, GA_ROOT));
        }
        public WindowInfo GetRootOwner()
        {
            return new WindowInfo(User32.GetAncestor(hWnd, GA_ROOTOWNER));
        }
        public WindowInfo GetParentNotOwner()
        {
            return new WindowInfo(User32.GetAncestor(hWnd, GA_PARENT));
        }
        public WindowInfo GetParent()
        {
            return new WindowInfo(User32.GetParent(hWnd));
        }
        public bool IsChild(WindowInfo parent)
        {
            return User32.IsChild(parent.Handle, hWnd);
        }
        public bool IsParent(WindowInfo child)
        {
            return User32.IsChild(hWnd, child.Handle);
        }
        public bool Minimized
        {
            get { return User32.IsIconic(hWnd); }
        }
        public bool Maximized
        {
            get
            {
                if (Minimized)
                {
                    WINDOWPLACEMENT wp = default(WINDOWPLACEMENT);
                    wp.Length = Marshal.SizeOf(wp);
                    User32.GetWindowPlacement(hWnd, ref wp);
                    return (wp.flags != 0);
                }
                else
                {
                    return User32.IsZoomed(hWnd);
                }
            }
        }
        public void BringToFront()
        {
            if (User32.SetForegroundWindow(hWnd) == false)
                throw new Exception("Error bringing window to front.");
        }
        public void SendToBack()
        {
            if (User32.SetWindowPos(hWnd, (IntPtr)HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE) == false)
                throw new Exception("Error sending window to back.");
        }
        public void SetAsTopMost()
        {
            if (User32.SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE) == false)
                throw new Exception("Error making window topmost.");
        }
        public void SetAsNotTopMost()
        {
            if (User32.SetWindowPos(hWnd, (IntPtr)HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE) == false)
                throw new Exception("Error making window non-topmost.");
        }
        public Bitmap CaptureBitmap()
        {
            Bitmap b = new Bitmap(Math.Max(1, this.Width), Math.Max(1, this.Height), PixelFormat.Format32bppArgb);
            Graphics gr = Graphics.FromImage(b);
            gr.Clear(Color.Transparent);
            gr.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.Clear(Color.Transparent);
            User32.PrintWindow(hWnd, gr.GetHdc(), 0);
            gr.ReleaseHdc();
            gr.Dispose();
            return b;
        }

        private const int GA_PARENT = 1;
        private const int GA_ROOT = 2;
        private const int GA_ROOTOWNER = 3;
        private const int HWND_TOP = 0;
        private const int HWND_BOTTOM = 1;
        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOREDRAW = 0x8;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_SHOWWINDOW = 0x40;
        private const int SWP_HIDEWINDOW = 0x80;
        private const int SWP_NOCOPYBITS = 0x100;
        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOSENDCHANGING = 0x400;
        private const int GWL_EXSTYLE = (-20);
        private const int GW_OWNER = 4;
        private const int WM_PRINT = 0x317;
        private const int PRF_NONCLIENT = 0x2;
        private const int PRF_CLIENT = 0x4;
        private const int PRF_ERASEBKGND = 0x8;
        private const int PRF_CHILDREN = 0x10;
        private const int PRF_OWNED = 0x20;




        public enum ChildFromPointOptions
        {
            All = 0,
            SkipInvisible = 1,
            SkipDisabled = 2,
            SkipTransparent = 4
        }
        public enum SendMessageTimeoutFlags : int
        {
            Normal = 0x0,
            Block = 0x1,
            AbortIfHung = 0x2,
            NoTimeoutIfNotHung = 0x8,
            ErrorOnExit = 0x20
        }
        public enum WindowState
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximized = 3,
            NormalNA = 4,
            Show = 5,
            Minimize = 6,
            ShowMinimizeNA = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 1
        }

        public enum WindowIconSize : int
        {
            Small = 0,
            Big = 1,
            Small2 = 2
        }
    }
}