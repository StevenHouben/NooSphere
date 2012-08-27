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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NooSphere.Platform.Windows.Hooks;
using System.Runtime.InteropServices;
using NooSphere.Platform.Windows.Windowing;

namespace NooSphere.Platform.Windows.VDM
{
    public class VirtualDesktop
    {
        #region Private Members

        /// <summary>
        /// Collection of windows that are located in the virtual desktop
        /// </summary>
        private Collection<WindowInfo> windows = new Collection<WindowInfo>();

        /// <summary>
        /// List of all windows
        /// </summary>
        private Collection<WindowInfo> windowsRestore = new Collection<WindowInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// Name of the virtual desktop
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates if the virtual desktop is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Returns a list of all windows that are located in the virtual desktop
        /// </summary>
        public Collection<WindowInfo> Windows
        {
            get
            {
                if (Active)
                {
                    return VirtualDesktopManager.GetCurrentWindows();
                }
                else
                {
                    Collection<WindowInfo> remItems = new Collection<WindowInfo>();
                    lock (windows)
                    {
                        foreach (WindowInfo w in windows)
                        {
                            if (!WindowManager.IsValid(w))
                            {
                                remItems.Add(w);
                            }
                        }
                        foreach (WindowInfo w in remItems)
                        {
                            windows.Remove(w);
                        }
                    }
                    return windows;
                }
            }
            set
            {
                this.windows = value;
            }
        }

        /// <summary>
        /// Returns a list of all ongoing processes
        /// </summary>
        public Collection<WindowInfo> WindowsToRestore
        {
            get { return windowsRestore; }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Validates a window that needs to be shown
        /// </summary>
        /// <param name="w">The windoinfo of the window that needs to be validated</param>
        /// <returns>A boolean indicating the validity of the window</returns>
        protected bool ShowWindowValid(WindowInfo w)
        {
            return VirtualDesktopManager.IsWindowValid(w, false, true, false);
        }

        /// <summary>
        /// Validates a window that needs to be hidden
        /// </summary>
        /// <param name="w">The windoinfo of the window that needs to be validated</param>
        /// <returns>A boolean indicating the validity of the window</returns>
        protected bool HideWindowValid(WindowInfo w)
        {
            return VirtualDesktopManager.IsWindowValid(w, false, false, true);
        }

        /// <summary>
        /// Matches a collection of windows to a given matcher
        /// </summary>
        /// <param name="coll">The collection of windows that need to be matched</param>
        /// <param name="matcher">A WindowInfo.WindowChooseDelegate delegate</param>
        /// <returns>A collection of windowinfo that match the matcher</returns>
        protected Collection<WindowInfo> Match(Collection<WindowInfo> coll,WindowChooseDelegate matcher)
        {
            Collection<WindowInfo> ret = new Collection<WindowInfo>();
            foreach (WindowInfo w in coll)
            {
                if (matcher(w))
                    ret.Add(w);
            }
            return ret;
        }

        #endregion

        #region Public Methods

        public void AddToWindowList(WindowInfo winf)
        {
            this.windows.Add(winf);
        }

        /// <summary>
        /// Shows the windows that are located in the virtual desktop
        /// </summary>
        /// <param name="switchBackground">Boolean that indicates if the desktop wallpaper needs to
        /// switch</param>
        public void ShowWindows(bool switchBackground)
        {
            lock (windows)
            {
                if (this.Active)
                    return;
                foreach (WindowInfo w in Match(Windows, ShowWindowValid))
                {
                    try
                    {
                        w.State = WindowInfo.WindowState.Show;
                        if (windowsRestore.Contains(w))
                            w.State = WindowInfo.WindowState.Restore;
                        w.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                try
                {
                    if (Windows.Count > 0)
                        Windows[Windows.Count - 1].BringToFront();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                windows.Clear();
                windowsRestore.Clear();
                Active = true;
            }
        }

        /// <summary>
        /// Hides all windows that are located in the virtual desktop
        /// </summary>
        public void HideWindows()
        {
            lock (windows)
            {
                windows.Clear();
                foreach (WindowInfo w in WindowManager.GetWindows(HideWindowValid))
                {
                    windows.Add(w);
                    foreach (VirtualDesktop v in VirtualDesktopManager.Desktops)
                    {
                        if (!object.ReferenceEquals(v, this))
                        {
                            v.RemoveAllInstances(w);
                        }
                    }
                    if (VirtualDesktopManager.IsProcessMinimizing(w.Process.ProcessName))
                    {
                        w.State = WindowInfo.WindowState.Minimize;
                        windowsRestore.Add(w);
                    }
                    w.State = WindowInfo.WindowState.Hide;
                }
                Active = false;
            }
        }

        /// <summary>
        /// Removes all instances of a window
        /// </summary>
        /// <param name="rw">Windowinfo of the window that needs to be removed</param>
        public void RemoveAllInstances(WindowInfo rw)
        {
            lock (windows)
            {
                System.Collections.ObjectModel.Collection<WindowInfo> remItems = new System.Collections.ObjectModel.Collection<WindowInfo>();
                foreach (WindowInfo w in windows)
                {
                    if (w == rw)
                    {
                        remItems.Add(w);
                    }
                }
                foreach (WindowInfo w in windowsRestore)
                {
                    if (w == rw)
                    {
                        remItems.Add(w);
                    }
                }
                foreach (WindowInfo w in remItems)
                {
                    this.windows.Remove(w);
                }
            }
        }

        #endregion
    }
}
