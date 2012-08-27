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
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ActivityUI.Properties;
using ActivityUI.Xaml;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Platform.Windows.Interopt;

namespace ActivityUI.PopUp
{
    /// <summary>
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private readonly ActivityBar taskbar;

        public ManagerWindow(ActivityBar bar)
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

        public void Show(int offset)
        {
            Left = offset;
            Top = taskbar.Height + 5;

            txtDeviceName.Text = Settings.Default.DEVICE_NAME;
            txtEmail.Text = Settings.Default.USER_EMAIL;
            txtUsername.Text = Settings.Default.USER_NAME;

            chkBroadcast.IsChecked = Settings.Default.CHECK_BROADCAST;

            RunDiscovery();
            Show();
        }

        public void RunDiscovery()
        {
            managerlist.Items.Clear();

            var t = new Thread(() =>
                                   {
                                       var disc = new DiscoveryManager();
                                       disc.DiscoveryAddressAdded += disc_DiscoveryAddressAdded;
                                       disc.Find(Settings.Default.DISCOVERY_TYPE);
                                   });
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Adds a discovered activity manager to the UI
        /// </summary>
        /// <param name="serviceInfo">The information of the found service</param>
        private void AddDiscoveryActivityManagerToUI(ServiceInfo serviceInfo)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                                                                            {
                                                                                managerlist.Items.Add(serviceInfo.Name +
                                                                                                      " at " +
                                                                                                      serviceInfo.
                                                                                                          Address);
                                                                                managerlist.MouseDoubleClick +=
                                                                                    managerlist_MouseDoubleClick;
                                                                            }));
        }

        private void managerlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(managerlist.SelectedItem.ToString());
        }

        private void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            AddDiscoveryActivityManagerToUI(e.ServiceInfo);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RunDiscovery();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var msg = new Message() { From = "steven", To = "all", Header = "Controller.RoleChanged", Content = txtInput.Text };
            taskbar.SendMessage(msg);
        }

        private void txtAddFriend_Click(object sender, RoutedEventArgs e)
        {
            taskbar.AddFriend(txtEmailFriend.Text);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Settings.Default.DEVICE_NAME = txtDeviceName.Text;
            Settings.Default.USER_NAME = txtUsername.Text;
            Settings.Default.USER_EMAIL = txtEmail.Text;
            Settings.Default.DISCOVERY_BROADCAST = (bool) chkBroadcast.IsChecked;
            Settings.Default.Save();
        }

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
    }
}