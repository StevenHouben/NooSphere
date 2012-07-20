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
using NooSphere.Core.Devices;
using NooSphere.Core.ActivityModel;
using ActivityUI.Properties;
using NooSphere.Helpers;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using ActivityUI.PopUp;

namespace ActivityUI.Login
{
    public partial class LoginWindow : Window
    {
        public event EventHandler LoggedIn = null;
        public Device Device { get; set; }
        public User User { get; set; }
        public StartUpMode Mode { get; set; } 
        public LoginWindow()
        {
            SourceInitialized += new EventHandler(LoginWindow_SourceInitialized);
            InitializeComponent();
            LoadSettings();

            ToolTipService.SetIsEnabled(btnInfo, false);
            ToolTipService.SetIsEnabled(btnStop, false);
            ToolTipService.SetIsEnabled(btnGo, false);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
        private void LoadSettings()
        {
            txtUsername.Text = Settings.Default.USER_NAME;
            txtEmail.Text = Settings.Default.USER_EMAIL;
            txtDevicename.Text = Settings.Default.USER_DEVICENAME;
        }
        private void SaveSettings()
        {
            Settings.Default.USER_NAME = txtUsername.Text;
            Settings.Default.USER_EMAIL = txtEmail.Text;
            Settings.Default.USER_DEVICENAME = txtDevicename.Text;
            Settings.Default.Save();
        }

        protected void OnLoggedIn()
        {
            if (LoggedIn != null)
                LoggedIn(this, new EventArgs());
        }
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            string baseUrl = "http://activitycloud-1.apphb.com/Api/";
            string result = RestHelper.Get(baseUrl + "Users?email=" + txtEmail.Text);
            User u = JsonConvert.DeserializeObject<User>(result);
            if (u != null)
                this.User = u;
            else
                CreateUser(baseUrl);

            this.Device = new Device();
            this.Device.Name = txtDevicename.Text;

            if (rbClientAndHost.IsChecked == true)
                this.Mode = StartUpMode.Host;
            else
                this.Mode = StartUpMode.Client;

            if (chkRemember.IsChecked==true)
                SaveSettings();
            OnLoggedIn();
        }
        private void CreateUser(string baseUrl)
        {
            User user = new User();
            user.Email = txtEmail.Text;
            user.Name = txtUsername.Text;
            string added = RestHelper.Post(baseUrl + "Users", user);
            if (JsonConvert.DeserializeObject<bool>(added))
            {
                var result = RestHelper.Get(baseUrl + "Users?email=" + txtEmail.Text);
                var u = JsonConvert.DeserializeObject<User>(result);
                this.User = u;
            }
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"http://activitycloud-1.apphb.com/");
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

    }
}
