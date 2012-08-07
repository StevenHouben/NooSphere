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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ActivityUI.Xaml;
using NooSphere.Platform.Windows.Interopt;
using ActivityUI.Properties;
using System.Threading;
using System.Windows.Threading;
using NooSphere.ActivitySystem.Discovery;

namespace ActivityUI.PopUp
{
    /// <summary>
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private ActivityBar taskbar;
        public ManagerWindow(ActivityBar bar)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.CanResize;
            this.MinHeight = this.MaxHeight = this.Height;
            this.MinWidth = this.MaxWidth = this.Width;
            this.taskbar = bar;
        }

        public void Show(int offset)
        {
            this.Left = offset;
            this.Top = taskbar.Height + 5;

            this.txtDeviceName.Text = Settings.Default.DEVICE_NAME;
            this.txtEmail.Text = Settings.Default.USER_EMAIL;
            this.txtUsername.Text = Settings.Default.USER_NAME;

            this.chkBroadcast.IsChecked = Settings.Default.CHECK_BROADCAST;

            RunDiscovery();
            this.Show();
        }
        public void RunDiscovery()
        {
            managerlist.Items.Clear();

            Thread t = new Thread(() =>
            {
                DiscoveryManager disc = new DiscoveryManager();
                disc.Find(DiscoveryType.Zeroconf);
                disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
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
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                managerlist.Items.Add(serviceInfo.Name + " at " + serviceInfo.Address);
                managerlist.MouseDoubleClick += new MouseButtonEventHandler(managerlist_MouseDoubleClick);
            }));
        }

        void managerlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(managerlist.SelectedItem.ToString());
        }

        void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            AddDiscoveryActivityManagerToUI(e.ServiceInfo);
        }

        #region Window Hacks

        private const int GWL_STYLE = -16;
        private const uint WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            User32.SetWindowLong(hwnd, GWL_STYLE,
                User32.GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));

            base.OnSourceInitialized(e);
        }
        #endregion

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RunDiscovery();
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            taskbar.SendMessage(txtInput.Text);
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
            Settings.Default.DISCOVERY_BROADCAST = (bool)chkBroadcast.IsChecked;
            Settings.Default.Save();
        }

    }
}
