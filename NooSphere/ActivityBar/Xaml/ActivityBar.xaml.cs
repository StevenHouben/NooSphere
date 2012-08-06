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

using NooSphere.ActivitySystem.ActivityClient;
using NooSphere.ActivitySystem.ActivityManager;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.ActivitySystem.Discovery.Client;
using NooSphere.ActivitySystem.Discovery.Primitives;
using NooSphere.ActivitySystem.Host;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Platform.Windows.Glass;
using NooSphere.Platform.Windows.VDM;

using ActivityUI.Properties;
using ActivityUI.Login;
using ActivityUI.PopUp;
using NooSphere.Platform.Windows.Hooks;

namespace ActivityUI
{
    public partial class ActivityBar : Window
    {
        #region Private Members

        private Client client;
        private BasicHost host;
        private DiscoveryManager disc;

        private StartUpMode startMode;

        private User owner;
        private Device device;
        private Activity currentActivity;

        private Dictionary<Guid, Proxy> proxies = new Dictionary<Guid, Proxy>();
        private ObservableCollection<User> contactList = new ObservableCollection<User>();
        private ObservableCollection<ServiceInfo> serviceList = new ObservableCollection<ServiceInfo>();
        private Dictionary<string, Device> deviceList = new Dictionary<string, Device>();

        private List<Window> PopUpWindows = new List<Window>();

        private ActivityButton currentButton;

        private LoginWindow login;
        private ActivityWindow activityWindow;
        private DeviceWindow deviceWindow;
        private ManagerWindow managerWindow;
        private StartMenu StartMenu;

        private bool startingUp = true;

        public RenderStyle RenderStyle { get; set; }
        public bool ClickDetected = false;

        #endregion

        #region Constructor
        /// <summary>
        /// Disables the UI and runs the Login procedure
        /// </summary>
        public ActivityBar()
        {
            InitializeComponent();
            activityWindow = new ActivityWindow(this);
            PopUpWindows.Add(activityWindow);
            managerWindow = new ManagerWindow(this);
            PopUpWindows.Add(managerWindow);
            StartMenu = new ActivityUI.StartMenu(this);
            PopUpWindows.Add(StartMenu);
            deviceWindow = new DeviceWindow(this);
            PopUpWindows.Add(deviceWindow);

            MouseHook.Register();
            MouseHook.MouseClick+=new System.Windows.Forms.MouseEventHandler(MouseHook_MouseClick);

            DisableUI();

            login = new LoginWindow();
            login.LoggedIn += new EventHandler(login_LoggedIn);
            login.Show();
        }
        #endregion

        #region Public members
        public void AddEmptyActivity()
        {
            Activity act = GetInitializedActivity();
            this.AddActivityUI(act);
            VirtualDesktopManager.CurrentDesktop= proxies[act.Id].Desktop;
        }
        /// <summary>
        /// Sends a message to the activity manager
        /// </summary>
        /// <param name="message">Message that needs to be send to the activity manager</param>
        public void SendMessage(string message)
        {
            client.SendMessage(message);
            txtInput.Text = "";
        }

        public void AddFriend(string email)
        {
            client.RequestFriendShip(email);
        }

        /// <summary>
        /// Runs the discovery manager to find managers on the local network
        /// </summary>
        private  void RunDiscovery()
        {
            serviceList.Clear();

            Thread t = new Thread(() =>
            {
                disc = new DiscoveryManager();
                disc.Find(DiscoveryType.ZEROCONF);
                disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
            });
            t.IsBackground = true;
            t.Start();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates if a global mouse click is in a given window
        /// </summary>
        /// <param name="w">The window</param>
        /// <param name="p">The global mouse click location</param>
        /// <returns></returns>
        private bool HitTest(Window w, System.Drawing.Point p)
        {
            if (w.Visibility == Visibility.Hidden)
                return false;
            else
                return (p.X >= w.Left && p.X <= w.Left + w.Width) && (p.Y >= w.Top && p.Y <= w.Top + w.Height);
        }
        private bool HitTestAllPopWindow(System.Drawing.Point location)
        {
            foreach (Window w in PopUpWindows)
                if (HitTest(w, location))
                    return true;
            return false;
        }

        /// <summary>
        /// Initializes the device, user and system
        /// </summary>
        private void IntializeSystem()
        {
            device = login.Device;
            device.Location = "pIT lab";

            this.deviceList.Add(device.Id.ToString(),device);
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
        /// Starts an activity manager service in an bacic http service host
        /// </summary>
        public void StartActivityManager()
        {
            Thread t = new Thread(() =>
            {
                host = new BasicHost();
                host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                host.Open(new ActivityManager(owner,"c:/files/"), typeof(IActivityManager), device.Name);
                if(Settings.Default.CHECK_BROADCAST)
                    host.StartBroadcast(device.Name, device.Location);

            });
            t.IsBackground = true ;
            t.Start();
        }

        /// <summary>
        /// Starts the activity client based on the host address
        /// </summary>
        public void StartClient()
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
            client = new Client(activityManagerHttpAddress, @"c:/abc/");

            //Register the current device with the activity manager we are connecting to
            client.Register(this.device);

            //Set the current user
            client.CurrentUser = owner;

            //Subscribe to the activity manager events
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.UserEvent);

            //Subcribe to the callback events of the activity manager
            client.DeviceAdded += new NooSphere.ActivitySystem.Events.DeviceAddedHandler(client_DeviceAdded);
            client.ActivityAdded += new NooSphere.ActivitySystem.Events.ActivityAddedHandler(client_ActivityAdded);
            client.ActivityChanged += new NooSphere.ActivitySystem.Events.ActivityChangedHandler(client_ActivityChanged);
            client.DeviceRemoved +=new NooSphere.ActivitySystem.Events.DeviceRemovedHandler(client_DeviceRemoved);
            client.ActivityRemoved += new NooSphere.ActivitySystem.Events.ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new NooSphere.ActivitySystem.Events.MessageReceivedHandler(client_MessageReceived);

            client.FriendAdded += new NooSphere.ActivitySystem.Events.FriendAddedHandler(client_FriendAdded);
            client.FriendDeleted += new NooSphere.ActivitySystem.Events.FriendDeletedHandler(client_FriendDeleted);
            client.FriendRequestReceived += new NooSphere.ActivitySystem.Events.FriendRequestReceivedHandler(client_FriendRequestReceived);

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

            List<User> users = client.GetUsers();
            if(users != null)
                contactList = new ObservableCollection<User>(users );

            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                friendList.ItemsSource = contactList;
            }));

            //DEBUG_AddActivities(50);
            //DEBUG_DeleteAllActivities();
        }

        /// <summary>
        /// Debug function -> remove when done
        /// </summary>
        private void DEBUG_DeleteAllActivities()
        {
            foreach (Proxy p in proxies.Values.ToList())
                client.RemoveActivity(p.Activity.Id);
        }

        /// <summary>
        /// Debug function -> remove when done
        /// </summary>
        private void DEBUG_AddActivities(int number)
        {
            for (int i = 0; i < number; i++)
                client.AddActivity(GetInitializedActivity());
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

                ActivityButton b = new ActivityButton(new Uri("pack://application:,,,/Images/activity.PNG"),activity.Name);
                b.RenderMode = RenderMode.Image;
                b.Click += new RoutedEventHandler(b_Click);
                b.MouseDown += new MouseButtonEventHandler(b_MouseDown);
                b.MouseEnter += new MouseEventHandler(b_MouseEnter);
                b.MouseLeave += new MouseEventHandler(b_MouseLeave);
                b.Height = this.Height - 5;
                b.ActivityId = p.Activity.Id;

                //b.Template = (ControlTemplate)this.FindResource("Dark");
                b.Style = (Style)FindResource("ColorHotTrackButton");

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
        private Proxy GetProxyFromButton(ActivityButton b)
        {
            return proxies[(Guid)b.ActivityId];
        }

        /// <summary>
        /// Shows the activity button context menu
        /// </summary>
        /// <param name="sender">The activity button that is right clicked</param>
        private void ShowActivityButtonContextMenu(ActivityButton btn)
        {
            if(activityWindow.Visibility == System.Windows.Visibility.Visible)
                activityWindow.Hide();
            GeneralTransform transform = btn.TransformToAncestor(this);
            Point rootPoint = transform.Transform(new Point(0, 0));

            activityWindow.Show(proxies[btn.ActivityId].Activity, (int)rootPoint.X);
        }

        private void ShowDeviceMenu(Button btn)
        {
            if (deviceWindow.Visibility == System.Windows.Visibility.Visible)
                activityWindow.Hide();
            GeneralTransform transform = btn.TransformToAncestor(this);
            Point rootPoint = transform.Transform(new Point(0, 0));

            deviceWindow.Show((int)rootPoint.X, deviceList.Values.ToList());

        }

        /// <summary>
        /// Shows the activity manager context menu
        /// </summary>
        private void ShowManagerContextMenu()
        {
            HideAllPopups();
            if (managerWindow.Visibility == System.Windows.Visibility.Visible)
                managerWindow.Hide();
            GeneralTransform transform = btnManager.TransformToAncestor(this);
            Point rootPoint = transform.Transform(new Point(0, 0));

            managerWindow.Show((int)rootPoint.X);
        }

        /// <summary>
        /// Hides the start menu
        /// </summary>
        private void UpdateDiscovery()
        {
            device.Name = txtDeviceName.Text;
            if (Settings.Default.CHECK_BROADCAST)
                host.StartBroadcast(device.Name, device.Location);
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
        /// Deletes the activity
        /// </summary>
        public void DeleteActivity()
        {
            Guid g = GetProxyFromButton(currentButton).Activity.Id;
            client.RemoveActivity(g);
        }

        /// <summary>
        /// Edits the content of an activity and
        /// updates the activity client.
        /// </summary>
        /// <param name="ac"></param>
        public void EditActivity(Activity ac)
        {
            currentActivity = proxies[(Guid)currentButton.ActivityId].Activity;
            currentButton.Text = ac.Name;
            client.UpdateActivity(ac);
        }

        /// <summary>
        /// Toggles the start menu
        /// </summary>
        private void ShowStartMenu()
        {
            HideAllPopups();
            StartMenu.Show();
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
        /// Initializes the taskbar
        /// </summary>
        private void InitializeTaskbar()
        {
            //Get pointer to our own application
            IntPtr handle = new WindowInteropHelper(this).Handle;

            //Force glass style
            if(RenderStyle == ActivityUI.RenderStyle.Glass)
                ApplyGlass(handle);

            NooSphere.Platform.Windows.Dock.AppBarFunctions.SetAppBar(this, NooSphere.Platform.Windows.Dock.AppBarPosition.Top);
        }

        /// <summary>
        /// Exits the application after cleaning up all resource
        /// </summary>
        public void ExitApplication()
        {
            //Hide all popUps
            HideAllPopups();

            //Close the taskbar
            this.Close();

            //Close the client;
            client.UnSubscribeAll();

            //Close the host if running
            if(host.IsRunning)
                host.Close();

            //Uninitialize the virtual desktop manager
            VirtualDesktopManager.UninitDesktops();

            //Close the entire environment
            Environment.Exit(0);
        }

        /// <summary>
        /// Hides all popUps
        /// </summary>
        private void HideAllPopups()
        {
            PopUpWindows.ForEach( w => w.Hide());
        }
        #endregion

        #region Event Handlers
        private void MouseHook_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(!HitTestAllPopWindow(e.Location))
                HideAllPopups();
        }

        private void btnManager_Click(object sender, RoutedEventArgs e)
        {
            ShowManagerContextMenu();
        }
        private void client_FriendRequestReceived(object sender, NooSphere.ActivitySystem.Events.FriendEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Do you want to add " + e.User.Name + " to your friend list?", "Friend list", MessageBoxButton.YesNo))
            {
                client.RespondToFriendRequest(e.User.Id, true);
            }
            else
                client.RespondToFriendRequest(e.User.Id, false);
        }
        private void client_FriendDeleted(object sender, NooSphere.ActivitySystem.Events.FriendDeletedEventArgs e)
        {

        }
        private void client_FriendAdded(object sender, NooSphere.ActivitySystem.Events.FriendEventArgs e)
        {

        }
        private void txtAddFriend_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void client_ActivityChanged(object sender, NooSphere.ActivitySystem.Events.ActivityEventArgs e)
        {
            proxies[e.Activity.Id].Activity = e.Activity;
            proxies[e.Activity.Id].Button.Text = e.Activity.Name; 
        }
        private void b_MouseLeave(object sender, MouseEventArgs e)
        {
            ((ActivityButton)sender).RenderMode = RenderMode.Image;
        }
        private void b_MouseEnter(object sender, MouseEventArgs e)
        {
            //((ActivityButton)sender).RenderMode = RenderMode.ImageAndText;
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RunDiscovery();
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
            HideAllPopups();
            if (e.RightButton == MouseButtonState.Pressed)
            {
                currentButton = (ActivityButton)sender;
                ShowActivityButtonContextMenu((ActivityButton)sender);
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
        private void client_DeviceRemoved(object sender, NooSphere.ActivitySystem.Events.DeviceRemovedEventArgs e)
        {
            deviceList.Remove(e.Id);
            AddToLog("Device Removed\n");
        }
        private void client_DeviceAdded(object sender, NooSphere.ActivitySystem.Events.DeviceEventArgs e)
        {
            deviceList.Add(e.Device.Id.ToString(),e.Device);
            AddToLog("Device Added\n");
        }
        private void client_MessageReceived(object sender, NooSphere.ActivitySystem.Events.ComEventArgs e)
        {
            AddToLog(e.Message+"\n");
        }
        private void client_ActivityRemoved(object sender, NooSphere.ActivitySystem.Events.ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.ID);
            AddToLog("Activity Removed\n");
        }
        private void client_ActivityAdded(object obj, NooSphere.ActivitySystem.Events.ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
            AddToLog("Activity Added\n");
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
            SwitchToVirtualDesktop(proxies[(Guid)((ActivityButton)sender).ActivityId].Desktop);
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            bool isShown = (StartMenu.Visibility == System.Windows.Visibility.Visible);
            HideAllPopups();
            if(!isShown)
                ShowStartMenu();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTaskbar();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
        private void popActivityManagers_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateDiscovery();
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

            User part = new User();
            part.Email = "test@test.dk";

            NooSphere.Core.ActivityModel.Action act = new NooSphere.Core.ActivityModel.Action();
            Resource res = new Resource();
            res.RelativePath = "/abc.txt";
            res.Size = (int)new FileInfo(client.LocalPath + res.RelativePath).Length;
            res.Name = "abc.txt";
            res.ActivityId = ac.Id;
            res.ActionId = act.Id;
            act.Resources.Add(res);


            ac.Actions.Add(act);
            ac.Participants.Add(part);
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
                this.Background = Brushes.Transparent;
                HwndSource mainWindowSrc = System.Windows.Interop.HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                DWM_BLURBEHIND blurBehindParameters = new DWM_BLURBEHIND();
                blurBehindParameters.dwFlags = DwmBlurBehindFlags.DWM_BB_ENABLE;
                blurBehindParameters.fEnable = true;
                blurBehindParameters.hRgnBlur = IntPtr.Zero;

                IntPtr result = DwmEnableBlurBehindWindow(handle, ref blurBehindParameters);
            }
        }
        #endregion

        private void btnPhone_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            ShowDeviceMenu((Button)sender);
        }

    }
    public enum RenderStyle
    {
        Glass,
        Plain
    }
}
