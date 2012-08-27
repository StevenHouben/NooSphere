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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.ActivitySystem.Host;
using NooSphere.Core.ActivityModel;
using Newtonsoft.Json;
using ActivityTablet.Properties;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery;

namespace ActivityTablet.Xaml
{
    public partial class Tablet
    {
        #region Private Members
        private ActivityClient _client;
        private GenericHost _host;
        private User _user;
        private Device _device;
        private readonly Dictionary<Guid, Proxy> proxies = new Dictionary<Guid, Proxy>();

        //private PointerNode _pNode = new PointerNode(PointerRole.Controller);
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
                string result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
                User u = JsonConvert.DeserializeObject<User>(result);
                if (u != null)
                    this._user = u;
                else
                    CreateUser(baseUrl);

                this._device = new Device();
                this._device.Name = txtDevicename.Text;

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
            try
            {
                Thread t = new Thread(() =>
                {
                    DiscoveryManager disc = new DiscoveryManager();
                    disc.DiscoveryAddressAdded += new DiscoveryManager.DiscoveryAddressAddedHandler(DiscDiscoveryAddressAdded);
                    disc.Find();
                });
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }
        private void StartActivityManager()
        {
            Task.Factory.StartNew(
                delegate {
                _host = new GenericHost();
                _host.HostLaunched += new HostLaunchedHandler(HostHostLaunched);
                _host.Open(new ActivityManager(_user, "c:/files/"), typeof(IActivityManager), "Tablet manager");
                _host.StartBroadcast(DiscoveryType.WSDiscovery, "Tablet", "205");
            });
        }
        private void BuildUI()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                cvLogin.Visibility = System.Windows.Visibility.Hidden;
                cvActivityManager.Visibility = System.Windows.Visibility.Visible;
                //contentBrowser.Navigate(@"http://itu.dk/people/shou/pubs/SituatedActivityModelMODIQUITOUS2012.pdf");
            }));

      

        }
        private void AddActivityUI(Activity ac)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                var p = new Proxy {Activity = ac};

                var b = new ActivityButton(new Uri("pack://application:,,,/Images/activity.PNG"), ac.Name)
                            {RenderMode = RenderMode.Image};
                b.TouchDown += new EventHandler<TouchEventArgs>(b_TouchDown);
                b.Click += new RoutedEventHandler(BClick);
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
        private void RemoveActivityUI(Guid id)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                ActivityDock.Children.Remove(proxies[id].Button);
                proxies.Remove(id);
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
            string added = Rest.Post(baseUrl + "Users", user);
            if (JsonConvert.DeserializeObject<bool>(added))
            {
                var result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
                var u = JsonConvert.DeserializeObject<User>(result);
                this._user = u;
            }
        }
        private void StartClient(string addr)
        {
            if (_client != null)
                return;
            try
            {
                _client = new ActivityClient(@"c:/abc/", _device) { CurrentUser = new User() };
                _client.MessageReceived += ClientMessageReceived;
                _client.ActivityAdded += ClientActivityAdded;
                _client.ActivityRemoved += ClientActivityRemoved;
                _client.ConnectionEstablished += ClientConnectionEstablished;
                _client.FileAdded += _client_FileAdded;
                _client.Open(addr);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        void _client_FileAdded(object sender, FileEventArgs e)
        {
            try
            {
                var image = new BadImageFormatException()
            }
            catch (Exception)
            {
                //not an image -> do better implementation here
            }
        }

        private void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.Id);
        }
        private void ClientMessageReceived(object sender, ComEventArgs e)
        {
            HandleMessage(e.Message);
        }
        private void ClientConnectionEstablished(object sender, EventArgs e)
        {
            BuildUI();
        }
        private void BtnQuitClick(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void HandleMessage(Message message)
        {
            if(message.Type==MessageType.Connect)
            {
                _client = null;
                StartClient(message.Content);
            }
        }
        #endregion

        #region Public Methods

        private void ClientActivityAdded(object obj, ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
        }
        private void BClick(object sender, RoutedEventArgs e)
        {
            _client.SwitchActivity(proxies[((ActivityButton)sender).ActivityId].Activity);
        }
        private void b_TouchDown(object sender, TouchEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void DiscDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            StartClient(e.ServiceInfo.Address);
        }
        private void BtnGoClick(object sender, RoutedEventArgs e)
        {
            LogIn();
        }
        private void HostHostLaunched(object sender, EventArgs e)
        {
            StartClient(_host.Address);
        }
        private void ExitApplication()
        {
            if (_client != null)
                _client.Close();
            Environment.Exit(0);
        }
        #endregion

    }
}