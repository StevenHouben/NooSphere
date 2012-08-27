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
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using ActivityUI.Xaml;
using NooSphere.Core.ActivityModel;
using NooSphere.Platform.Windows.Interopt;

namespace ActivityUI.PopUp
{
    /// <summary>
    /// Interaction logic for PopUpWindow.xaml
    /// </summary>
    public partial class ActivityWindow : Window
    {
        #region Window Hacks

        private const int GWL_STYLE = -16;
        private const uint WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetWindowLong(hwnd, GWL_STYLE,
                                 User32.GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));

            base.OnSourceInitialized(e);
        }

        #endregion

        private readonly ActivityBar taskbar;
        private Activity activity;

        public ActivityWindow(ActivityBar bar)
        {
            InitializeComponent();
            Topmost = true;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.CanResize;
            MinHeight = MaxHeight = Height;
            MinWidth = MaxWidth = Width;
            taskbar = bar;
        }

        public void Show(Activity act, int offset)
        {
            activity = act;
            Left = offset;
            Top = taskbar.Height + 5;

            txtActivity.Text = act.Name;
            txtInfo.Text = act.Id.ToString();

            Show();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            activity.Name = txtActivity.Text;
            //other stuff
            taskbar.EditActivity(activity);
            Hide();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            taskbar.DeleteActivity();
            Hide();
        }
    }
}