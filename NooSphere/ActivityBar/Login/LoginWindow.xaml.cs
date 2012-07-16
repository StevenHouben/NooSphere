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
    }
}
