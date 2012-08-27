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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Specialized;
using NooSphere.Platform.Windows.Hooks;
using System.Runtime.InteropServices;
using System.IO;
using NooSphere.Platform.Windows.Windowing;

namespace NooSphere.Platform.Windows.VDM
{
    public class VirtualDesktopManager
    {
        //Settings Replacement
        static string UseMonitors = "";
        static StringCollection DesktopNames;
        static StringCollection StickyPrograms;
        static StringCollection MinimizePrograms;
        static bool initDone = false;
        //end settings replacement

        #region Delegates
        /// <summary>
        /// The desktop switcher delegate
        /// </summary>
        public delegate void VirtualDesktopSwitchedEventHandler();

        #endregion

        #region Events

        /// <summary>
        /// Handles the VirtualDesktopSwitched event
        /// </summary>
        public static event VirtualDesktopSwitchedEventHandler VirtualDesktopSwitched;

        #endregion

        #region Members

        /// <summary>
        /// A collection of desktops that are managed by the virtual desktop manager
        /// </summary>
        private static Collection<VirtualDesktop> _desktops = new Collection<VirtualDesktop>();
        
        /// <summary>
        /// A list of monitors
        /// </summary>
        private static int[] useMons;

        /// <summary>
        /// The desktop that is currently selected
        /// </summary>
        private static VirtualDesktop _currentDesktop;

        /// <summary>
        /// Collection of windows that do not belong to any specific
        /// virtual desktop and are therefor displayed on all virtual
        /// desktops
        /// </summary>
        private static Collection<WindowInfo> _stickyWindows = new Collection<WindowInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// The readonly collection of desktops that are managed by the
        /// virtual desktop manager
        /// </summary>
        public static Collection<VirtualDesktop> Desktops
        {
            get { return _desktops; }
        }

        /// <summary>
        /// The desktop that is currently selected
        /// </summary>
        public static VirtualDesktop CurrentDesktop
        {
            get { return _currentDesktop; }
            set
            {
                if (_currentDesktop != value)
                {
                    //Make sure we don't have any copies
                    Collection<WindowInfo> windows = _currentDesktop.Windows;
                    foreach (WindowInfo w in windows)
                    {
                        foreach (VirtualDesktop d in Desktops)
                        {
                            d.RemoveAllInstances(w);
                        }
                    }
                    //Switch the windows
                    int prevDesk = CurrentDesktopIndex;


                    _currentDesktop.HideWindows();

                    _currentDesktop = value;
                    value.ShowWindows(true);
                    if (VirtualDesktopSwitched != null)
                    {
                        VirtualDesktopSwitched();
                    }
                }
            }
        }

        /// <summary>
        /// The index from the collection of desktops of the
        /// desktop that is currently selected
        /// </summary>
        public static int CurrentDesktopIndex
        {
            get { return Desktops.IndexOf(CurrentDesktop); }
            set { VirtualDesktopManager.CurrentDesktop = VirtualDesktopManager.Desktops[value]; }
        }

        /// <summary>
        /// A readonly list of monitors
        /// </summary>
        public static int[] MonitorIndices
        {
            get { return useMons; }
        }


        /// <summary>
        /// A collection of stickywindows
        /// </summary>
        public static Collection<WindowInfo> StickyWindows
        {
            get
            {
                for (int i = 0; i <= _stickyWindows.Count - 1; i++)
                {
                    if (!WindowManager.IsValid(_stickyWindows[i]))
                    {
                        _stickyWindows.RemoveAt(i);
                        i -= 1;
                    }
                }
                return _stickyWindows;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the virtual desktop manager.
        /// </summary>
        /// <param name="num">The number of desktops the user wants
        /// to initialize</param>
        public static void InitDesktops(int num)
        {
            //Initialize settings
            if (num < 1)
                num = 1;
            if (DesktopNames == null)
            {
                DesktopNames = new StringCollection();
                for (int i = 0; i < num; i++)
                    DesktopNames.Add("Desktop" + (i + 1).ToString());
            }
            if (StickyPrograms == null)
            {
                StickyPrograms = new StringCollection();
                StickyPrograms.Add("sidebar");
                StickyPrograms.Add("taskmgr");
            }
            if (MinimizePrograms == null)
            {
                MinimizePrograms = new StringCollection();
            }

            //Only use select monitors
            StringCollection useMonStrs = new StringCollection();
            useMonStrs.AddRange(UseMonitors.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            if (useMons == null)
                useMons = new int[4];
            else
                useMons = new int[useMons.Count()];

            for (int i = 0; i <= useMonStrs.Count - 1; i++)
            {
                useMons[i] = int.Parse(useMonStrs[i]);
            }

            //Add desktops
            for (int i = 0; i <= num - 1; i++)
            {
                _desktops.Add(new VirtualDesktop());
                if (DesktopNames.Count > i)
                {
                    _desktops[i].Name = DesktopNames[i];
                }
                else
                {
                    _desktops[i].Name = "Desktop" + " " + (i + 1).ToString();
                }
            }
            _currentDesktop = _desktops[0];
            _currentDesktop.ShowWindows(false);
            initDone = true;
        }

        public static void SendWindowToDesktop(WindowInfo w, int newDesk, int oldDesk)
        {
            if (oldDesk == newDesk)
                return;
            if (oldDesk == VirtualDesktopManager.CurrentDesktopIndex)
            {
                try
                {
                    VirtualDesktopManager.Desktops[newDesk].Windows.Add(w);
                    if (!w.Minimized && VirtualDesktopManager.IsProcessMinimizing(w.Process.ProcessName))
                    {
                        w.State = WindowInfo.WindowState.Minimize;
                        w.BringToFront();
                        VirtualDesktopManager.Desktops[newDesk].WindowsToRestore.Add(w);
                    }
                    w.State = WindowInfo.WindowState.Hide;

                }
                catch {}
            }
            else if (newDesk == VirtualDesktopManager.CurrentDesktopIndex)
            {
                try
                {
                    w.State = WindowInfo.WindowState.ShowNA;
                    if (VirtualDesktopManager.Desktops[oldDesk].WindowsToRestore.Contains(w))
                        w.State = WindowInfo.WindowState.Restore;
                    VirtualDesktopManager.Desktops[oldDesk].RemoveAllInstances(w);

                }
                catch{}
            }
            else
            {
                VirtualDesktopManager.Desktops[newDesk].Windows.Add(w);
                if (VirtualDesktopManager.Desktops[oldDesk].WindowsToRestore.Contains(w))
                {
                    VirtualDesktopManager.Desktops[newDesk].WindowsToRestore.Add(w);
                }
                VirtualDesktopManager.Desktops[oldDesk].RemoveAllInstances(w);
            }
        }

        public static void SendWindowToDesktop(WindowInfo w, int newDesk)
        {
           int sourceDesk = FindSourceDesk(w);
           if(sourceDesk!=-1)
           {
                SendWindowToDesktop(w, newDesk,sourceDesk);
           }
        }
        public static void ShowOnAllDesktops(WindowInfo w)
        {
            foreach (VirtualDesktop d in VirtualDesktopManager.Desktops)
            {
                try
                {
                    w.State = WindowInfo.WindowState.ShowNA;
                    d.Windows.Add(w);

                }
                catch { }
            }
        }

        private static int FindSourceDesk(WindowInfo w)
        {
            for(int i=0;i<VirtualDesktopManager.Desktops.Count;i++)
            {
                VirtualDesktop d = VirtualDesktopManager.Desktops[i];
                foreach (WindowInfo winf in d.Windows)
                {
                    if (w.ProcessId == winf.ProcessId)
                        return i;
                }
            }
            return -1;
        }

        public static Collection<WindowInfo> GetAllWindows()
        {
            Collection<WindowInfo> win =new Collection<WindowInfo>();
            foreach (VirtualDesktop d in Desktops)
            {
                foreach (WindowInfo w in d.Windows)
                {
                    bool found = false;
                    foreach (WindowInfo winf in win)
                    {
                        if (w.ProcessId == winf.ProcessId)
                            found = true;
                    }
                    if(!found)
                        win.Add(w);
                }
            }
            return win;
        }
        public static void ShowWindow(WindowInfo winf)
        {
            foreach (VirtualDesktop d in Desktops)
            {
                foreach (WindowInfo w in d.Windows)
                {
                    if ((!object.ReferenceEquals(w, winf)))
                        winf.State = WindowInfo.WindowState.Show;
                }
            }
        }
        public static void HideWindow(WindowInfo winf)
        {
            foreach (VirtualDesktop d in Desktops)
            {
                foreach (WindowInfo w in d.Windows)
                {
                    if ((!object.ReferenceEquals(w, winf)))
                        winf.State = WindowInfo.WindowState.Hide;
                }
            }
        }


        /// <summary>
        /// Unintializes the virtual desktop manager
        /// </summary>
        public static void UninitDesktops()
        {
            if (initDone)
            {
                foreach (VirtualDesktop d in Desktops)
                {
                    if ((!object.ReferenceEquals(CurrentDesktop, d)))
                    d.ShowWindows(false);

                }
                Desktops.Clear();
            }
        }

        /// <summary>
        /// Gets the current windows
        /// </summary>
        /// <returns>Collection of windows</returns>
        public static Collection<WindowInfo> GetCurrentWindows()
        {
            return WindowManager.GetWindows(IsWindowValid);
        }

        /// <summary>
        /// Checks the validity of a window
        /// </summary>
        /// <param name="w">The windowinfo of the window</param>
        /// <returns>A boolean indicating the validity of the window</returns>
        public static bool IsWindowValid(WindowInfo w)
        {
            return IsWindowValid(w, false);
        }

        /// <summary>
        /// Checks if a process is sticky
        /// </summary>
        /// <param name="p">Name of the process</param>
        /// <returns>A boolean indicating if the process is sticky</returns>
        public static bool IsProcessSticky(string p)
        {
            foreach (string s in StickyPrograms)
            {
                try
                {
                    Regex reg = new Regex("\\*" + p.ToLower() + "\\*");
                    Regex reg2 = new Regex("\\*" + s.ToLower() + "\\*");
                    if (reg.IsMatch(s.ToLower()) | reg2.IsMatch(p.ToLower()))
                    {

                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a process is minimized
        /// </summary>
        /// <param name="p">The name of the process</param>
        /// <returns>A boolean indicating if the process is minimized</returns>
        public static bool IsProcessMinimizing(string p)
        {
            foreach (string s in MinimizePrograms)
            {
                Regex reg = new Regex("\\*" + p.ToLower() + "\\*");
                Regex reg2 = new Regex("\\*" + s.ToLower() + "\\*");
                if (reg.IsMatch(s.ToLower()) || reg2.IsMatch(p.ToLower()))
                {

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the window is valid
        /// </summary>
        /// <param name="w">The windowinfo of the window</param>
        /// <param name="includeStickies">A boolean indicating if the sticky
        /// window should be included into the check</param>
        /// <returns>A boolean indicating if the window is valid</returns>
        public static bool IsWindowValid(WindowInfo w, bool includeStickies)
        {
            bool includeHidden = false;
            bool includeShadows = false;
            Regex reg = new Regex("\\*explorer\\*");
            if (WindowManager.IsValid(w)
                && (includeStickies || (!IsSticky(w)))
                && (w.Text != "" || (includeShadows && (w.ClassName == "SysShadow" || w.ClassName == "ShadowWindow")))
                && (includeHidden || w.Visible)
                && (!(w.Handle == WindowManager.FindWindowByClass("Progman").Handle))
                && (!(w.ClassName.ToUpper() == "BUTTON" && (w.Text == "Start" || w.Text == "Démarrer")))
                && (!(System.Diagnostics.Debugger.IsAttached && w.ClassName == "wndclass_desked_gsk"))
                //&& (true || Array.IndexOf(useMons, (Array.IndexOf(Screen.AllScreens, Screen.FromHandle(w.Handle)))) >= 0)
                && w.Width > 0
                && w.Height > 0
                && (!(w.Text == "Start Menu" && reg.IsMatch(w.Process.ProcessName)))
                && (!(w.ClassName == "Desktop User Picture" && reg.IsMatch(w.Process.ProcessName)))
                && (!(w.ClassName == "ThumbnailClass" && reg.IsMatch(w.Process.ProcessName)))
                && (!IsProcessSticky(w.Process.ProcessName)) && (!(w.ProcessId == Process.GetCurrentProcess().Id)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool AllMonitors =  true;
        /// <summary>
        /// Checks if the window is valid
        /// </summary>
        /// <param name="w">The windowinfo of the window<</param>
        /// <param name="includeStickies">A boolean indicating if the sticky
        /// window should be included into the check</param>
        /// <param name="includeHidden">A boolean indicating if hidden windows
        /// should be included in the check</param>
        /// <param name="includeShadows">A boolean indicating if shadow
        /// windows should be included in the check</param>
        /// <returns>A boolean indicating if the window is valid</returns>
        public static bool IsWindowValid(WindowInfo w, bool includeStickies, bool includeHidden, bool includeShadows)
        {
            try
            {
                Regex reg = new Regex("\\*explorer\\*");
                if (WindowManager.IsValid(w)
                    && (includeStickies || (!IsSticky(w)))
                    && (w.Text != "" || (includeShadows && (w.ClassName == "SysShadow" || w.ClassName == "ShadowWindow")))
                    && (includeHidden || w.Visible)
                    && (!(w.Handle == WindowManager.FindWindowByClass("Progman").Handle))
                    && (!(w.ClassName.ToUpper() == "BUTTON" && (w.Text == "Start" || w.Text == "Démarrer")))
                    && (!(System.Diagnostics.Debugger.IsAttached && w.ClassName == "wndclass_desked_gsk"))
                    && (AllMonitors || Array.IndexOf(useMons, (Array.IndexOf(Screen.AllScreens, Screen.FromHandle(w.Handle)))) >= 0)
                    && w.Width > 0
                    && w.Height > 0
                    && (!(w.Text == "Start Menu" && reg.IsMatch(w.Process.ProcessName)))
                    && (!(w.ClassName == "Desktop User Picture" && reg.IsMatch(w.Process.ProcessName)))
                    && (!(w.ClassName == "ThumbnailClass" && reg.IsMatch(w.Process.ProcessName)))
                    && (!IsProcessSticky(w.Process.ProcessName)) && (!(w.ProcessId == Process.GetCurrentProcess().Id)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return true; }
        }

        /// <summary>
        /// Checks if a window is sticky
        /// </summary>
        /// <param name="w">The window info</param>
        /// <returns>A boolean indicating if the window is sticky</returns>
        public static bool IsSticky(WindowInfo w)
        {
            foreach (WindowInfo s in StickyWindows)
            {
                if (s == w)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Makes a window stick
        /// </summary>
        /// <param name="w">the windowinfo of the window</param>
        public static void StickWindow(WindowInfo w)
        {
            if (!IsSticky(w))
                StickyWindows.Add(w);
        }

        /// <summary>
        /// Makes a windows unstick
        /// </summary>
        /// <param name="w">The windowinfo of the window</param>
        public static void UnstickWindow(WindowInfo w)
        {
            if (IsSticky(w))
            {
                WindowInfo delI = null;
                foreach (WindowInfo s in StickyWindows)
                {
                    if (s == w)
                    {
                        delI = s;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
                if (delI != null)
                    StickyWindows.Remove(delI);
            }
        }

        #endregion

    }
    public class DesktopIcon
    {
        public Point Location { get; set; }
        public int DesktopIndex { get; set; }

        public DesktopIcon() { }
        public DesktopIcon(int index, Point location)
        {
            this.DesktopIndex = index;
            this.Location = location;
        }
    }
}
