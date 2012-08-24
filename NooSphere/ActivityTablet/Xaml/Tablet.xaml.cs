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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.ActivitySystem.Contracts.Service;
using NooSphere.ActivitySystem.Host;
using NooSphere.Context.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Contracts;
using Newtonsoft.Json;
using ActivityTablet.Properties;
using NooSphere.Helpers;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery;

namespace ActivityTablet
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
                    disc.DiscoveryAddressAdded += new DiscoveryManager.DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
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
            Thread t = new Thread(() =>
            {
                _host = new GenericHost();
                _host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                _host.StartBroadcast(DiscoveryType.WSDiscovery, "Tablet","204");
                _host.Open(new ActivityManager(_user, "c:/files/"), typeof(IActivityManager), "Tablet manager");
            });
            t.Start();
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

                _client.ActivityAdded += ClientActivityAdded;
                _client.ConnectionEstablished += new ConnectionEstablishedHandler(_client_ConnectionEstablished);
                _client.Open(addr);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
           


        }

        void _client_ConnectionEstablished(object sender, EventArgs e)
        {
            BuildUI();
            
        }
        #endregion

        #region Public Methods


        private void client_DeviceAdded(object sender, DeviceEventArgs e)
        {
        }
        private void client_MessageReceived(object sender, ComEventArgs e)
        {
        }
        private void client_ActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
        }
        private void ClientActivityAdded(object obj, ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
        }
        private void b_Click(object sender, RoutedEventArgs e)
        {
            _client.SwitchActivity(proxies[((ActivityButton)sender).ActivityId].Activity);
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
            StartClient(_host.Address);
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (_client != null)
                _client.Close();
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