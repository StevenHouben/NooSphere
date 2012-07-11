using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Platform.Windows.Interopt;
using System.Drawing;

namespace NooSphere.Platform.Windows.Windowing
{
    public delegate bool WindowChooseDelegate(WindowInfo w);

    public class WindowManager
    {
        public static WindowInfo FindWindowByClass(string className)
        {
            return FindWindow(className, null);
        }
        public static WindowInfo FindWindowByText(string windowText)
        {
            return FindWindow(null, windowText);
        }
        public static WindowInfo FindWindow(string classname, string windowText)
        {
            IntPtr h = User32.FindWindow(classname, windowText);
            if (IsValid(h))
            {
                return new WindowInfo(h);
            }
            else
            {
                return null;
            }
        }


        private static System.Collections.ObjectModel.Collection<WindowInfo> infos;
        public static System.Collections.ObjectModel.Collection<WindowInfo> GetWindows()
        {
            infos = new System.Collections.ObjectModel.Collection<WindowInfo>();
            User32.EnumWindows(ListWindow, 0);
            return infos;
        }


        static WindowChooseDelegate chooser;

        public static System.Collections.ObjectModel.Collection<WindowInfo> GetWindows(WindowChooseDelegate windowChoose)
        {
            chooser = windowChoose;
            System.Collections.ObjectModel.Collection<WindowInfo> r = GetWindows();
            chooser = null;
            return r;
        }

        private static bool ListWindow(IntPtr hwnd, int lparam)
        {
            if (chooser == null || chooser(new WindowInfo(hwnd)))
            {
                infos.Insert(0, new WindowInfo(hwnd));
            }
            return true;
        }

        public static WindowInfo GetDesktopWindow()
        {
            return new WindowInfo(User32.GetDesktopWindow());
        }

        public static WindowInfo GetForegroundWindow()
        {
            return new WindowInfo(User32.GetForegroundWindow());
        }

        public static WindowInfo GetWindowFromPoint(Point p)
        {
            APIPOINT ap = default(APIPOINT);
            ap.x = p.X;
            ap.y = p.Y;
            IntPtr h = User32.WindowFromPoint(ap);
            if (IsValid(h))
            {
                return new WindowInfo(h);
            }
            else
            {
                return null;
            }
        }

        public static bool IsValid(WindowInfo w)
        {
            return User32.IsWindow(w.Handle);
        }

        public static bool IsValid(IntPtr h)
        {
            return User32.IsWindow(h);
        }

    }
}
