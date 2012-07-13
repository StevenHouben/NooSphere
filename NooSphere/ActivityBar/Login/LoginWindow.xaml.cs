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
            InitializeComponent();
            LoadSettings();

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
            this.User = new User();
            this.User.Email = txtEmail.Text;
            this.User.Name = txtUsername.Text;

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
    }
}
