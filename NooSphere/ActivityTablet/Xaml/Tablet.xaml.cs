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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Threading;
using NooSphere.ActivitySystem.ActivityClient;
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.ActivityManager;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Contracts;
using Newtonsoft.Json;
using ActivityTablet.Properties;
using NooSphere.Helpers;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery.Client;

namespace ActivityTablet
{
    public partial class Tablet : Window
    {
        #region Private Members
        private Client client;
        private BasicHost host;
        private User user;
        private Device device;
        private Dictionary<Guid, Proxy> proxies = new Dictionary<Guid, Proxy>();
        #endregion

        #region Constructor
        public Tablet()
        {
            //Initializes design-time components
            InitializeComponent();
            LoadSettings();
        }
        #endregion

        #region Private Methods
        private void LogIn()
        {
            try
            {
                string baseUrl = Settings.Default.ENVIRONMENT_BASE_URL;
                string result = RestHelper.Get(baseUrl + "Users?email=" + txtEmail.Text);
                User u = JsonConvert.DeserializeObject<User>(result);
                if (u != null)
                    this.user = u;
                else
                    CreateUser(baseUrl);

                this.device = new Device();
                this.device.Name = txtDevicename.Text;

                if (chkClient.IsChecked == true)
                    FindClient();
                else
                    StartActivityManager();
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void FindClient()
        {
            RunDiscovery();
        }
        private void RunDiscovery()
        {
            Thread t = new Thread(() =>
            {
                DiscoveryManager disc = new DiscoveryManager();
                disc.Find();
                disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
            });
            t.IsBackground = true;
            t.Start();
        }
        private void StartActivityManager()
        {
            Thread t = new Thread(() =>
            {
                host = new BasicHost();
                host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                host.StartBroadcast("Tablet");
                host.Open(new ActivityManager(user, "c:/files/"), typeof(IActivityManager), "Tablet manager");
            });
            t.Start();
        }
        private void BuildUI()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                cvLogin.Visibility = System.Windows.Visibility.Hidden;
                cvActivityManager.Visibility = System.Windows.Visibility.Visible;
                contentBrowser.Navigate(@"http://itu.dk/people/shou/pubs/SituatedActivityModelMODIQUITOUS2012.pdf");
            }));
            List<Activity> lac = client.GetActivities();
            foreach (Activity ac in lac)
            {
                AddActivityUI(ac);
            }

      

        }
        private void AddActivityUI(Activity ac)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Proxy p = new Proxy();
                p.Activity = ac;

                ActivityButton b = new ActivityButton(new Uri("pack://application:,,,/Images/activity.PNG"), ac.Name);
                b.RenderMode = RenderMode.Image;
                b.TouchDown += new EventHandler<TouchEventArgs>(b_TouchDown);
                b.Click += new RoutedEventHandler(b_Click);
                b.Height = b.Width = 100;
                b.ActivityId = p.Activity.Id;
                b.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                b.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                b.Padding = new Thickness(10);
                b.Style = (Style)FindResource("ActivityTouchButton");

                p.Button = b;

                ActivityDock.Children.Add(b);
                proxies.Add(p.Activity.Id, p);
            }));
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
                this.user = u;
            }
        }
        private void StartClient(string addr)
        {
            client = new Client(addr, @"c:/abc/");

            //Register the current device with the activity manager we are connecting to
            client.Register();

            //Set the current user
            client.CurrentUser = user;

            //Subscribe to the activity manager events
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.UserEvent);

            //Subcribe to the callback events of the activity manager
            client.DeviceAdded += new NooSphere.ActivitySystem.Events.DeviceAddedHandler(client_DeviceAdded);
            client.ActivityAdded += new NooSphere.ActivitySystem.Events.ActivityAddedHandler(client_ActivityAdded);
            //client.ActivityChanged += new NooSphere.ActivitySystem.Events.ActivityChangedHandler(client_ActivityChanged);

            client.ActivityRemoved += new NooSphere.ActivitySystem.Events.ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new NooSphere.ActivitySystem.Events.MessageReceivedHandler(client_MessageReceived);

            //client.FriendAdded += new NooSphere.ActivitySystem.Events.FriendAddedHandler(client_FriendAdded);
            //client.FriendDeleted += new NooSphere.ActivitySystem.Events.FriendDeletedHandler(client_FriendDeleted);
            //client.FriendRequestReceived += new NooSphere.ActivitySystem.Events.FriendRequestReceivedHandler(client_FriendRequestReceived);

            BuildUI();

        }
        #endregion

        #region Public Methods


        private void client_DeviceAdded(object sender, NooSphere.ActivitySystem.Events.DeviceEventArgs e)
        {

        }
        private void client_MessageReceived(object sender, NooSphere.ActivitySystem.Events.ComEventArgs e)
        {
        }
        private void client_ActivityRemoved(object sender, NooSphere.ActivitySystem.Events.ActivityRemovedEventArgs e)
        {
        }
        private void client_ActivityAdded(object obj, NooSphere.ActivitySystem.Events.ActivityEventArgs e)
        {
        }
        private void b_Click(object sender, RoutedEventArgs e)
        {
               // throw new NotImplementedException();
        }
        private void b_TouchDown(object sender, TouchEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            StartClient(e.ServiceInfo.Address);
        }
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            LogIn();

        }
        private void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient(host.Address);
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (client != null)
                client.UnSubscribeAll();
            Environment.Exit(0);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnGo_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {

        }
        private void btnGo_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        private void btnGo_MouseLeave(object sender, MouseEventArgs e)
        {

        }
        private void activityTouchButton_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
    }
}