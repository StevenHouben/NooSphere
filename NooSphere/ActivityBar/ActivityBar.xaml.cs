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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using NooSphere.Platform.Windows.Glass;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;

using NooSphere.ActivitySystem.Host;
using NooSphere.Platform.Windows.VDM;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery.Client;
using NooSphere.ActivitySystem.Discovery.Primitives;

using ActivityUI.Properties;
using ActivityUI.Login;
using NooSphere.ActivitySystem.ActivityClient;
using NooSphere.ActivitySystem.ActivityManager;


namespace ActivityUI
{
    public partial class ActivityBar : Window
    {
        #region Private Members
        private BasicClient client;
        private BasicHost host;
        private DiscoveryManager disc;
        private StartUpMode startMode;
        private User owner;
        private Device device;
        private Dictionary<Guid, Proxy> proxies = new Dictionary<Guid, Proxy>();
        private Activity currentActivity;
        private Button currentButton;
        private LoginWindow login;
        private bool startingUp = true;
        #endregion

        #region Constructor
        /// <summary>
        /// Disables the UI and runs the Login procedure
        /// </summary>
        public ActivityBar()
        {
            InitializeComponent();
            DisableUI();

            login = new LoginWindow();
            login.LoggedIn += new EventHandler(login_LoggedIn);
            login.Show();
        }
        #endregion

        #region Private Members
        /// <summary>
        /// Initializes the device, user and system
        /// </summary>
        private void IntializeSystem()
        {
            device = login.Device;
            device.Location = "pIT lab";
            owner = login.User;
            startMode = login.Mode;
            InitializeNetwork();
        }

        /// <summary>
        /// Initializes the network 
        /// </summary>
        private void InitializeNetwork()
        {
            RunDiscovery();

            if (startMode == StartUpMode.Host)
                StartActivityManager();
            else
            {
                chkBroadcast.IsChecked = chkBroadcast.IsEnabled = false;
                Settings.Default.CHECK_BROADCAST = false;
            }
        }

        /// <summary>
        /// Runs the discovery manager to find managers on the local network
        /// </summary>
        private void RunDiscovery()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                managerlist.Items.Clear();
            }));

            disc = new DiscoveryManager();
            disc.Find();
            disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
        }

        /// <summary>
        /// Starts an activity manager service in an bacic http service host
        /// </summary>
        private void StartActivityManager()
        {
            Thread t = new Thread(() =>
            {
                host = new BasicHost();
                host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                host.Open(new ActivityManager(owner), typeof(IActivityManager), device.Name);
                if(Settings.Default.CHECK_BROADCAST)
                    host.StartBroadcast(device.Name, device.Location);

            });
            t.Start();
        }

        /// <summary>
        /// Starts the activity client based on the host address
        /// </summary>
        private void StartClient()
        {
            StartClient(host.Address);
        }

        /// <summary>
        /// Starts the activity client based on a given http address
        /// </summary>
        /// <param name="activityManagerHttpAddress"></param>
        private void StartClient(string activityManagerHttpAddress)
        {

            //Build a new client that connects to an activity manager on the given address
            client = new BasicClient(activityManagerHttpAddress);

            //Register the current device with the activity manager we are connecting to
            client.Register();

            //Set the current user
            client.CurrentUser = owner;

            //Subscribe to the activity manager events
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);

            //Subcribe to the callback events of the activity manager
            client.DeviceAdded += new NooSphere.Core.Events.DeviceAddedHandler(client_DeviceAdded);
            client.ActivityAdded += new NooSphere.Core.Events.ActivityAddedHandler(client_ActivityAdded);
            client.DeviceRemoved += new NooSphere.Core.Events.DeviceRemovedHandler(client_DeviceRemoved);
            client.ActivityRemoved += new NooSphere.Core.Events.ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new NooSphere.Core.Events.MessageReceivedHandler(client_MessageReceived);

            BuildUI();
            startingUp = false;
        }

        /// <summary>
        /// Disable the UI
        /// </summary>
        private void DisableUI()
        {
            ToggleUI(false);
        }

        /// <summary>
        /// Enable the UI
        /// </summary>
        private void EnableUI()
        {
            ToggleUI(true);
        }

        /// <summary>
        /// Toggle the UI
        /// </summary>
        /// <param name="d">Bool indicating if the UI is enable or disabled</param>
        private void ToggleUI(bool d)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                btnAdd.IsEnabled = d;
                btnHome.IsEnabled = d;
            }));
        }

        /// <summary>
        /// Builds the activity taskbar
        /// </summary>
        private void BuildUI()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                BuildDiscoveryUI();
            }));


            List<Activity> loc = client.GetActivities();
            if (loc != null)
            {
                foreach (Activity activity in loc)
                    AddActivityUI(activity);
            }
            EnableUI();
            VirtualDesktopManager.InitDesktops(1);
        }

        /// <summary>
        /// Builds the discovery UI
        /// </summary>
        private void BuildDiscoveryUI()
        {
            txtDeviceName.Text = device.Name;
            txtEmail.Text = owner.Email;
            txtUsername.Text = owner.Name;
        }

        /// <summary>
        /// Removes an activity button from the activity bar
        /// </summary>
        /// <param name="id">Guid value identifying the activity</param>
        private void RemoveActivityUI(Guid id)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Body.Children.Remove(proxies[id].Button);
            }));
        }

        /// <summary>
        /// Adds an activity button to the activity bar
        /// </summary>
        /// <param name="activity">The activity that is represented by the button</param>
        private void AddActivityUI(Activity activity)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Proxy p = new Proxy();
                p.Desktop = new VirtualDesktop();
                p.Activity = activity;
                VirtualDesktopManager.Desktops.Add(p.Desktop);

                Button b = new Button();
                b.Click += new RoutedEventHandler(b_Click);
                b.MouseDown += new MouseButtonEventHandler(b_MouseDown);
                b.Width = 300;
                b.Height = this.Height - 5;
                b.Tag = activity.Id;
                b.Content = activity.Name;

                p.Button = b;
                Body.Children.Add(p.Button);

                proxies.Add(p.Activity.Id, p);
            }));
        }

        /// <summary>
        /// Converts a button object into an activity proxy
        /// </summary>
        /// <param name="b">Button representing an activity</param>
        /// <returns>A proxy object that represent that activity connected to the button</returns>
        private Proxy GetProxyFromButton(Button b)
        {
            return proxies[(Guid)b.Tag];
        }

        /// <summary>
        /// Shows the activity button context menu
        /// </summary>
        /// <param name="sender"></param>
        private void ShowActivityButtonContextMenu(Button btn)
        {
            currentButton = btn;
            popupActivity.PlacementTarget = currentButton;
            currentActivity = proxies[(Guid)currentButton.Tag].Activity;
            popupActivity.IsOpen = !popupActivity.IsOpen;
            txtName.Text = currentActivity.Name;
            foreach (User u in currentActivity.Participants)
                txtParticipants.Text = u.Name;
        }

        /// <summary>
        /// Hides the activity button context menu
        /// </summary>
        private void HideActivityButtonContextMenu(bool deleted)
        {
            popupActivity.IsOpen = false;
            currentButton.Content = txtName.Text;
            currentActivity.Name = txtName.Text;
            if(!deleted) 
                client.UpdateActivity(currentActivity);
        }

        /// <summary>
        /// Hides the start menu
        /// </summary>
        private void HideStartMenu()
        {
            ToggleStartMenu();
            device.Name = txtDeviceName.Text;
            if (Settings.Default.CHECK_BROADCAST)
                host.StartBroadcast(device.Name, device.Location);
            RunDiscovery();
        }

        /// <summary>
        /// Checks if client wants to broadcast activity manager
        /// </summary>
        /// <param name="check"></param>
        private void CheckBroadCast(bool check)
        {
            if(startMode == StartUpMode.Host)
                if (check)
                    host.StartBroadcast(device.Name, device.Location);
                else
                    host.StopBroadcast();
        }

        /// <summary>
        /// Sends a message to the activity manager
        /// </summary>
        /// <param name="message">Message that needs to be send to the activity manager</param>
        private void SendMessage(string message)
        {
            client.SendMessage(message);
            txtInput.Text = "";
        }

        /// <summary>
        /// Deletes the activity
        /// </summary>
        private void DeleteActivity()
        {
            Button b = (Button)popupActivity.PlacementTarget;
            Guid g = GetProxyFromButton(b).Activity.Id;
            client.RemoveActivity(g);
            HideActivityButtonContextMenu(true);
        }

        /// <summary>
        /// Adds a discovered activity manager to the UI
        /// </summary>
        /// <param name="serviceInfo">The information of the found service</param>
        private void AddDiscoveryActivityManagerToUI(ServiceInfo serviceInfo)
        {
            if (startMode == StartUpMode.Client && startingUp)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Activity Manager found '" + serviceInfo.Name
                    + "' do you wish to connect to local activity service?",
               "Activity Manager Found", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                    StartClient(serviceInfo.Address);
                else
                    StartActivityManager();
            }
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                managerlist.Items.Add(serviceInfo.Name + " at " + serviceInfo.Address);
            }));
        }

        /// <summary>
        /// Adds a message to the UI log
        /// </summary>
        /// <param name="output"></param>
        private void AddToLog(string message)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                txtLog.Text = message + txtLog.Text;
            }));

        }

        /// <summary>
        /// Switches to a virtual desktop
        /// </summary>
        /// <param name="desktop">The virtual desktop to which the manager has to switch</param>
        private void SwitchToVirtualDesktop(VirtualDesktop desktop)
        {
            try
            {
                VirtualDesktopManager.CurrentDesktop = desktop;
            }
            catch
            {
                VirtualDesktopManager.UninitDesktops();
            }
        }

        /// <summary>
        /// Toggles the start menu
        /// </summary>
        private void ToggleStartMenu()
        {
            popActivityManagers.PlacementTarget = btnStart;
            popActivityManagers.IsOpen = !popActivityManagers.IsOpen;
        }

        /// <summary>
        /// Initializes the taskbar
        /// </summary>
        private void InitializeTaskbar()
        {
            //Get pointer to our own application
            IntPtr handle = new WindowInteropHelper(this).Handle;

            //Force glass style
            ApplyGlass(handle);

            NooSphere.Platform.Windows.Dock.AppBarFunctions.SetAppBar(this, NooSphere.Platform.Windows.Dock.AppBarPosition.Top);
        }
        #endregion

        #region Event Handlers
        private void btnDeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            DeleteActivity();
        }
        private void chkBroadcast_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.CHECK_BROADCAST = (bool)chkBroadcast.IsChecked;
            CheckBroadCast(Settings.Default.CHECK_BROADCAST);
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtInput.Text);
        }
        private void login_LoggedIn(object sender, EventArgs e)
        {
            login.Close();
            IntializeSystem();
        }
        private void b_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                ShowActivityButtonContextMenu((Button)sender);
            }
        }
        private void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient();
        }
        private void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            AddDiscoveryActivityManagerToUI(e.ServiceInfo);

        }
        private void client_DeviceRemoved(object sender, NooSphere.Core.Events.DeviceEventArgs e)
        {
            AddToLog("Device Removed\n");
        }
        private void client_DeviceAdded(object sender, NooSphere.Core.Events.DeviceEventArgs e)
        {
            AddToLog("Device Added\n");
        }
        private void client_MessageReceived(object sender, NooSphere.Core.Events.ComEventArgs e)
        {
            AddToLog(e.Message+"\n");
        }
        private void client_ActivityRemoved(object sender, NooSphere.Core.Events.ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.ID);
            AddToLog("Activity Added\n");
        }
        private void client_ActivityAdded(object obj, NooSphere.Core.Events.ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
            AddToLog("Activity Removed\n");
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            client.AddActivity(GetInitializedActivity());
        }
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            VirtualDesktopManager.CurrentDesktopIndex = 0;
        }
        private void b_Click(object sender, RoutedEventArgs e)
        {
            SwitchToVirtualDesktop(proxies[(Guid)((Button)sender).Tag].Desktop);
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ToggleStartMenu();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTaskbar();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            VirtualDesktopManager.UninitDesktops();
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            client.UnSubscribeAll();
        }
        private void popActivityManagers_MouseLeave(object sender, MouseEventArgs e)
        {
            HideStartMenu();
        }
        private void popupActivity_MouseLeave(object sender, MouseEventArgs e)
        {
            //HideActivityButtonContextMenu(false);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Generates a default activity
        /// </summary>
        /// <returns>An intialized activity</returns>
        public Activity GetInitializedActivity()
        {
            Activity ac = new Activity();
            ac.Name = "test activity - " + DateTime.Now;
            ac.Description = "This is the description of the test activity - " + DateTime.Now;
            ac.Uri = "http://tempori.org/" + ac.Id;

            ac.Context = "random context model here";
            ac.Meta.Data = "added meta data";

            ac.Owner = owner;

            NooSphere.Core.ActivityModel.Action act = new NooSphere.Core.ActivityModel.Action();
            ac.Actions.Add(act);

            return ac;
        }
        #endregion

        #region Taskbar Glass

        /// <summary>
        /// P/Invoke calls
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Desktop Window Manager Blur flags
        /// </summary>
        [Flags]
        private enum DwmBlurBehindFlags : uint
        {
            /// <summary>
            /// Indicates a value for fEnable has been specified.
            /// </summary>
            DWM_BB_ENABLE = 0x00000001,

            /// <summary>
            /// Indicates a value for hRgnBlur has been specified.
            /// </summary>
            DWM_BB_BLURREGION = 0x00000002,

            /// <summary>
            /// Indicates a value for fTransitionOnMaximized has been specified.
            /// </summary>
            DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
        }

        /// <summary>
        /// Deskop Window Manager Attributes
        /// </summary>
        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        /// <summary>
        /// Desktop Window Manager Policies
        /// </summary>
        [Flags]
        public enum DWMNCRenderingPolicy
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }


        /// <summary>
        /// Managed interpretation of native struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DwmBlurBehindFlags dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        /// <summary>
        /// Applies glass to the current window
        /// </summary>
        private void ApplyGlass(IntPtr handle)
        {
            if (DwmApi.DwmIsCompositionEnabled())
            {
                HwndSource mainWindowSrc = System.Windows.Interop.HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                DWM_BLURBEHIND blurBehindParameters = new DWM_BLURBEHIND();
                blurBehindParameters.dwFlags = DwmBlurBehindFlags.DWM_BB_ENABLE;
                blurBehindParameters.fEnable = true;
                blurBehindParameters.hRgnBlur = IntPtr.Zero;

                IntPtr result = DwmEnableBlurBehindWindow(handle, ref blurBehindParameters);
            }
            else
            {
                this.Background = SystemColors.WindowFrameBrush;
            }

        }
        #endregion

        private void btnApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            HideActivityButtonContextMenu(false);
        }
    }
}
