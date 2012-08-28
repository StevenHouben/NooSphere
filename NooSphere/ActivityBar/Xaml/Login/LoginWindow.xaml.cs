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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.Core.Devices;
using NooSphere.Core.ActivityModel;
using ActivityUI.Properties;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;

namespace ActivityUI.Xaml.Login
{
    public partial class LoginWindow : Window
    {
        #region Events
        public event EventHandler LoggedIn = null;
        #endregion

        #region Properties
        public Device Device { get; set; }
        public User User { get; set; }
        public StartUpMode Mode { get; set; }
        #endregion

        #region Constructor
        public LoginWindow()
        {
            SourceInitialized += new EventHandler(LoginWindow_SourceInitialized);
            InitializeComponent();

            for (int i = 0; i < 256; i++)
                cbTag.Items.Add(i);

            LoadSettings();

            ToolTipService.SetIsEnabled(btnInfo, false);
            ToolTipService.SetIsEnabled(btnStop, false);
            ToolTipService.SetIsEnabled(btnGo, false);

            this.cbType.ItemsSource = Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>();



        }
        #endregion

        #region Private Members
        private void LoadSettings()
        {
            txtUsername.Text = Settings.Default.USER_NAME;
            txtEmail.Text = Settings.Default.USER_EMAIL;
            txtDevicename.Text = Settings.Default.DEVICE_NAME;
            cbType.SelectedValue = Settings.Default.DEVICE_TYPE;
            cbTag.SelectedValue = Settings.Default.DEVICE_TAG;
        }
        private void SaveSettings()
        {
            Settings.Default.USER_NAME = txtUsername.Text;
            Settings.Default.USER_EMAIL = txtEmail.Text;
            Settings.Default.DEVICE_NAME = txtDevicename.Text;
            Settings.Default.DEVICE_TYPE = (DeviceType)cbType.SelectedValue;
            Settings.Default.DEVICE_TAG = (int)cbTag.SelectedValue;
            Settings.Default.Save();
        }
        private void CreateUser(string baseUrl)
        {
            User user = new User();
            user.Email = txtEmail.Text;
            user.Name = txtUsername.Text;
            string added = Rest.Post(baseUrl + "Users", user);
            if (JsonConvert.DeserializeObject<bool>(added))
            {
                var result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
                var u = JsonConvert.DeserializeObject<User>(result);
                this.User = u;
            }
        }
        private void LogIn()
        {
            txtTooltip.Text = "Please wait while we check your login.";
            string baseUrl = Settings.Default.ENVIRONMENT_BASE_URL;
            string result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
            User u = JsonConvert.DeserializeObject<User>(result);
            if (u != null)
                this.User = u;
            else
                CreateUser(baseUrl);

            this.Device = new Device();
            this.Device.Name = txtDevicename.Text;
            this.Device.DeviceType = (DeviceType)cbType.SelectedValue;
            Device.TagValue = (int)cbTag.SelectedValue;

            if (rbClient.IsChecked == true)
                this.Mode = StartUpMode.Client;
            else
                this.Mode = StartUpMode.Host;

            if (chkRemember.IsChecked == true)
                SaveSettings();

            if (LoggedIn != null)
                LoggedIn(this, new EventArgs());
        }
        #endregion

        #region Event Handlers
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            LogIn();
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void btnGo_MouseEnter(object sender, MouseEventArgs e)
        {
            this.txtTooltip.Foreground = ((Button)sender).BorderBrush;
            this.txtTooltip.Text = ((Button)sender).ToolTip.ToString();
        }
        private void btnGo_MouseLeave(object sender, MouseEventArgs e)
        {
            this.txtTooltip.Text = "";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"http://activitycloud-1.apphb.com/");
        }
        #endregion

        #region Window Extension
        void LoginWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            int style = GetWindowLong(wih.Handle, GWL_STYLE);
            SetWindowLong(wih.Handle, GWL_STYLE, style & ~WS_SYSMENU);
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0X00080000;

        [DllImport("user32.dll")]
        private extern static int SetWindowLong(IntPtr hwnd, int index, int value);
        [DllImport("user32.dll")]
        private extern static int GetWindowLong(IntPtr hwnd, int index);
        #endregion
    }
}
