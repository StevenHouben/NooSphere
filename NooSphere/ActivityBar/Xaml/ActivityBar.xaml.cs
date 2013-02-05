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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ActivityUI.Context;
using ActivityUI.Xaml.Login;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.ActivitySystem.Host;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Platform.Windows.Glass;
using NooSphere.Platform.Windows.Hooks;
using NooSphere.Platform.Windows.VDM;
using ActivityUI.Properties;
using ActivityUI.PopUp;

namespace ActivityUI.Xaml
{
    public partial class ActivityBar
    {
        #region Private Members

        private ActivityClient _client;
        private GenericHost _host;
        private DiscoveryManager _disc;

        private StartUpMode _startMode;

        private User _owner;
        private Device _device;

        private readonly Dictionary<Guid, Proxy> _proxies = new Dictionary<Guid, Proxy>();
        private readonly ObservableCollection<ServiceInfo> _serviceList = new ObservableCollection<ServiceInfo>();

        private readonly List<Window> _popUpWindows = new List<Window>();

        private ActivityButton _currentButton;

        private readonly LoginWindow _login;
        private readonly ActivityWindow _activityWindow;
        private readonly DeviceWindow _deviceWindow;
        private readonly ManagerWindow _managerWindow;
        private readonly StartMenu _startMenu;

        private bool _startingUp = true;

        public RenderStyle RenderStyle { get; set; }
        public bool ClickDetected = false;

        private string _startupDesktopPath;

        //Debug
        //private PointerNode _pointer = new PointerNode(PointerRole.Controller);


        #endregion

        #region Constructor
        /// <summary>
        /// Disables the UI and runs the Login procedure
        /// </summary>
        public ActivityBar()
        {
            _startupDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            InitializeComponent();
            _activityWindow = new ActivityWindow(this);
            _popUpWindows.Add(_activityWindow);
            _managerWindow = new ManagerWindow(this);
            _popUpWindows.Add(_managerWindow);
            _startMenu = new StartMenu(this);
            _popUpWindows.Add(_startMenu);
            _deviceWindow = new DeviceWindow(this);
            _popUpWindows.Add(_deviceWindow);

            //MouseHook.Register();
            //MouseHook.MouseDown += MouseHookMouseClick;
            //MouseHook.MouseMove += MouseHookMouseMove;

            DisableUi();

            _login = new LoginWindow();
            _login.LoggedIn += LoginLoggedIn;
            _login.Show();
        }
        #endregion

        #region Public members
        public void AddEmptyActivity()
        {
            var act = GetInitializedActivity();
            _client.AddActivity(act);
        }
        /// <summary>
        /// Sends a message to the activity manager
        /// </summary>
        /// <param name="message">Message that needs to be send to the activity manager</param>
        public void SendMessage(Message message)
        {
            _client.SendMessage(message);
            txtInput.Text = "";
        }

        public void AddFriend(string email)
        {
            _client.RequestFriendShip(email);
        }

        /// <summary>
        /// Runs the discovery manager to find managers on the local network
        /// </summary>
        private  void RunDiscovery()
        {
            _serviceList.Clear();

         Task.Factory.StartNew(
                delegate 
                {
                        _disc = new DiscoveryManager();
                        _disc.DiscoveryAddressAdded += DiscDiscoveryAddressAdded;
                        _disc.Find(Settings.Default.DISCOVERY_TYPE);
                    });
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
            return (p.X >= w.Left && p.X <= w.Left + w.Width) && (p.Y >= w.Top && p.Y <= w.Top + w.Height);
        }

        /// <summary>
        /// Runs a hittest on all registered windows
        /// </summary>
        /// <param name="location">The click point</param>
        /// <returns>Bool that indicates if a popup window was hit</returns>
        private bool HitTestAllPopWindow(System.Drawing.Point location)
        {
            return _popUpWindows.Any(w => HitTest(w, location));
        }

        /// <summary>
        /// Initializes the device, user and system
        /// </summary>
        private void IntializeSystem()
        {
            _device = _login.Device;
            _device.Location = "pIT lab";

            _owner = _login.User;
            _startMode = _login.Mode;
            InitializeNetwork();
        }

        /// <summary>
        /// Initializes the network 
        /// </summary>
        private void InitializeNetwork()
        {
            RunDiscovery();

            if (_startMode == StartUpMode.Host)
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
            Task.Factory.StartNew(
                   delegate
                   {
                       _host = new GenericHost(7891);
                        _host.HostLaunched += HostHostLaunched;
                        _host.Open(new ActivityManager(_owner, "c:/files/"), typeof (IActivityManager), _device.Name);
                        _host.StartBroadcast(Settings.Default.DISCOVERY_TYPE, _device.Name, _device.TagValue.ToString(), _device.Location);

                    });
        }

        /// <summary>
        /// Starts the activity client based on the host address
        /// </summary>
        public void StartClient()
        {
            StartClient(_host.Address);
        }

        /// <summary>
        /// Starts the activity client based on a given http address
        /// </summary>
        /// <param name="activityManagerHttpAddress"></param>
        private void StartClient(string activityManagerHttpAddress)
        {
            _client = new ActivityClient( @"c:/abc/",_device) {CurrentUser = _owner};

            _client.ActivityAdded += ClientActivityAdded;
            _client.ActivityChanged += ClientActivityChanged;
            _client.ActivityRemoved += ClientActivityRemoved;
            _client.ActivitySwitched += ClientActivitySwitched;

            _client.MessageReceived += ClientMessageReceived;

            _client.FriendAdded += client_FriendAdded;
            _client.FriendDeleted += client_FriendDeleted;
            _client.FriendRequestReceived += ClientFriendRequestReceived;

            _client.ConnectionEstablished += ClientConnectionEstablished;
            _client.ServiceIsDown += ClientServiceIsDown;
            _client.ContextMonitor.AddContextService(new InputRedirect(PointerRole.Controller));
            _client.ContextMessageReceived += _client_ContextMessageReceived;

            _client.Open(activityManagerHttpAddress);
        }

        void ClientActivitySwitched(object sender, ActivityEventArgs e)
        {
            VirtualDesktopManager.CurrentDesktop = _proxies[e.Activity.Id].Desktop;

            var activityFolder = _client.LocalPath+ e.Activity.Id;
            if (Directory.Exists(activityFolder))
                DesktopFolderSwitcher.ChangeDesktopFolder(activityFolder);
        }

        void ClientServiceIsDown(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        void _client_ContextMessageReceived(object sender, ContextEventArgs e)
        {
            string[] coords = e.Message.Split('-');

            Point point = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
            //SetCursorPos((int)point.Y, (int)point.X);
        }

        void ClientConnectionEstablished(object sender, EventArgs e)
        {
            _client.ContextMonitor.Start();
            BuildUi();
            _startingUp = false;
        }

        /// <summary>
        /// Disable the UI
        /// </summary>
        private void DisableUi()
        {
            ToggleUi(false);
        }

        /// <summary>
        /// Enable the UI
        /// </summary>
        private void EnableUi()
        {
            ToggleUi(true);
        }

        /// <summary>
        /// Toggle the UI
        /// </summary>
        /// <param name="d">Bool indicating if the UI is enable or disabled</param>
        private void ToggleUi(bool d)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                btnAdd.IsEnabled = d;
                btnHome.IsEnabled = d;
            }));
        }

        /// <summary>
        /// Builds the activity taskbar
        /// </summary>
        private void BuildUi()
        {
            EnableUi();
            VirtualDesktopManager.InitDesktops(1);
        }

        /// <summary>
        /// Removes an activity button from the activity bar
        /// </summary>
        /// <param name="id">Guid value identifying the activity</param>
        private void RemoveActivityUi(Guid id)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            { if (Body.Children != null) Body.Children.Remove(_proxies[id].Button); }));
        }

        /// <summary>
        /// Adds an activity button to the activity bar
        /// </summary>
        /// <param name="activity">The activity that is represented by the button</param>
        private void AddActivityUi(Activity activity)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                var p = new Proxy {Desktop = new VirtualDesktop(), Activity = activity};
                VirtualDesktopManager.Desktops.Add(p.Desktop);

                var b = new ActivityButton(new Uri("pack://application:,,,/Images/activity.PNG"),activity.Name) 
                { RenderMode = RenderMode.Image, ActivityId = p.Activity.Id };
                b.AllowDrop = true;
                b.Drop += BDrop;
                b.Click += BClick;
                b.MouseDown += BMouseDown;
                b.Style = (Style)FindResource("ColorHotTrackButton");

                p.Button = b;
                Body.Children.Add(p.Button);

                _proxies.Add(p.Activity.Id, p);
            }));
        }

        /// <summary>
        /// Converts a button object into an activity proxy
        /// </summary>
        /// <param name="b">Button representing an activity</param>
        /// <returns>A proxy object that represent that activity connected to the button</returns>
        private Proxy GetProxyFromButton(ActivityButton b)
        {
            return _proxies[b.ActivityId];
        }

        /// <summary>
        /// Shows the activity button context menu
        /// </summary>
        /// <param name="btn">The activity button that is right clicked</param>
        private void ShowActivityButtonContextMenu(ActivityButton btn)
        {
            if(_activityWindow.Visibility == Visibility.Visible)
                _activityWindow.Hide();
            var transform = btn.TransformToAncestor(this);
            var rootPoint = transform.Transform(new Point(0, 0));

            _activityWindow.Show(_proxies[btn.ActivityId].Activity, (int)rootPoint.X);
        }

        private void ShowDeviceMenu(Button btn)
        {
            if (_deviceWindow.Visibility == Visibility.Visible)
                _activityWindow.Hide();
            var transform = btn.TransformToAncestor(this);
            var rootPoint = transform.Transform(new Point(0, 0));

            _deviceWindow.Show((int)rootPoint.X, _client.DeviceList.Values.ToList());

        }

        /// <summary>
        /// Shows the activity manager context menu
        /// </summary>
        private void ShowManagerContextMenu()
        {
            HideAllPopups();
            if (_managerWindow.Visibility == Visibility.Visible)
                _managerWindow.Hide();
            var transform = btnManager.TransformToAncestor(this);
            var rootPoint = transform.Transform(new Point(0, 0));

            _managerWindow.Show((int)rootPoint.X);
        }

        /// <summary>
        /// Hides the start menu
        /// </summary>
        private void UpdateDiscovery()
        {
            _device.Name = txtDeviceName.Text;
            if (Settings.Default.CHECK_BROADCAST)
                _host.StartBroadcast(Settings.Default.DISCOVERY_TYPE,_device.Name, _device.Location);
        }

        /// <summary>
        /// Checks if client wants to broadcast activity manager
        /// </summary>
        /// <param name="check"></param>
        private void CheckBroadCast(bool check)
        {
            if(_startMode == StartUpMode.Host)
                if (check)
                    _host.StartBroadcast(Settings.Default.DISCOVERY_TYPE,_device.Name, _device.Location);
                else
                    _host.StopBroadcast();
        }

        /// <summary>
        /// Deletes the activity
        /// </summary>
        public void DeleteActivity()
        {
            Guid g = GetProxyFromButton(_currentButton).Activity.Id;
            _client.RemoveActivity(g);
        }

        /// <summary>
        /// Edits the content of an activity and
        /// updates the activity client.
        /// </summary>
        /// <param name="ac"></param>
        public void EditActivity(Activity ac)
        {
            _currentButton.Text = ac.Name;
            _client.UpdateActivity(ac);
        }

        /// <summary>
        /// Toggles the start menu
        /// </summary>
        private void ShowStartMenu()
        {
            HideAllPopups();
            _startMenu.Show();
        }

        /// <summary>
        /// Adds a discovered activity manager to the UI
        /// </summary>
        /// <param name="serviceInfo">The information of the found service</param>
        private void AddDiscoveryActivityManagerToUi(ServiceInfo serviceInfo)
        {
            if (_startMode == StartUpMode.Client && _startingUp)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Activity Manager found '" + serviceInfo.Name
                    + "' do you wish to connect to local activity service?",
               "Activity Manager Found", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                    StartClient(serviceInfo.Address);
                else
                    StartActivityManager();
            }
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() 
                => managerlist.Items.Add(serviceInfo.Name + " at " + serviceInfo.Address)));
        }

        /// <summary>
        /// Adds a message to the UI log
        /// </summary>
        /// <param name="message">The message that needs to be added to the log</param>
        private void AddToLog(string message)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
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
            if(RenderStyle == RenderStyle.Glass)
                ApplyGlass(handle);

            NooSphere.Platform.Windows.Dock.AppBarFunctions.SetAppBar(this, NooSphere.Platform.Windows.Dock.AppBarPosition.Top);
        }

        /// <summary>
        /// Exits the application after cleaning up all resource
        /// </summary>
        public void ExitApplication()
        {

            DesktopFolderSwitcher.ChangeDesktopFolder(_startupDesktopPath);
            //Hide all popUps
            HideAllPopups();

            if (_startMode == StartUpMode.Client)
                _client.Close();

            //Close the host if running
            if(_host != null &&_host.IsRunning)
                _host.Close();

            //Close the taskbar
            Close();

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
            _popUpWindows.ForEach( w => w.Hide());
        }
        #endregion

        #region Event Handlers
        private void BtnPhoneClick(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            ShowDeviceMenu((Button)sender);
        }
        private void MouseHookMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(!HitTestAllPopWindow(e.Location))
                HideAllPopups();
        }
        private void MouseHookMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!HitTestAllPopWindow(e.Location))
                HideAllPopups();
                HideAllPopups();
            //_client.SendContext(e.Location.X + "$" + e.Location.Y);
        }
        private void BtnManagerClick(object sender, RoutedEventArgs e)
        {
            ShowManagerContextMenu();
        }
        private void ClientFriendRequestReceived(object sender, FriendEventArgs e)
        {
            _client.RespondToFriendRequest(e.User.Id,
                                           MessageBoxResult.Yes ==
                                           MessageBox.Show(
                                               "Do you want to add " + e.User.Name + " to your friend list?",
                                               "Friend list", MessageBoxButton.YesNo));
        }
        private void client_FriendDeleted(object sender, FriendDeletedEventArgs e)
        {

        }
        private void client_FriendAdded(object sender, FriendEventArgs e)
        {

        }
        private void TxtAddFriendClick(object sender, RoutedEventArgs e)
        {
            
        }
        private void ClientActivityChanged(object sender, ActivityEventArgs e)
        {
            _proxies[e.Activity.Id].Activity = e.Activity;
            _proxies[e.Activity.Id].Button.Text = e.Activity.Name; 
        }
        private void BtnRefreshClick(object sender, RoutedEventArgs e)
        {
            RunDiscovery();
        }
        private void ChkBroadcastClick(object sender, RoutedEventArgs e)
        {
            if (chkBroadcast.IsChecked != null) Settings.Default.CHECK_BROADCAST = (bool)chkBroadcast.IsChecked;
            CheckBroadCast(Settings.Default.CHECK_BROADCAST);
        }
        private void BtnSendClick(object sender, RoutedEventArgs e)
        {

        }
        private void LoginLoggedIn(object sender, EventArgs e)
        {
            _login.Close();
            IntializeSystem();
        }
        private void BMouseDown(object sender, MouseButtonEventArgs e)
        {
            HideAllPopups();
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _currentButton = (ActivityButton)sender;
                ShowActivityButtonContextMenu((ActivityButton)sender);
            }
        }
        private void HostHostLaunched(object sender, EventArgs e)
        {
            StartClient();
        }
        private void DiscDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            AddDiscoveryActivityManagerToUi(e.ServiceInfo);
        }
        private void ClientMessageReceived(object sender,ComEventArgs e)
        {
            AddToLog(e.Message+"\n");
        }
        private void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            RemoveActivityUi(e.Id);
            AddToLog("Activity Removed\n");
        }
        private void ClientActivityAdded(object obj,ActivityEventArgs e)
        {
            AddActivityUi(e.Activity);
            Console.WriteLine("Activity Added\n");

            //_client.AddResource(new FileInfo("c:/dump/abc" + count.ToString() + ".jpg"), e.Activity.Id);
            //count++;
        }

        private int count = 1;
        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            AddEmptyActivity();
        }
        private void BtnHomeClick(object sender, RoutedEventArgs e)
        {
            VirtualDesktopManager.CurrentDesktopIndex = 0;
        }
        private void BClick(object sender, RoutedEventArgs e)
        {
            _client.SwitchActivity(_proxies[((ActivityButton) sender).ActivityId].Activity);
            //SwitchToVirtualDesktop(_proxies[((ActivityButton)sender).ActivityId].Desktop);
        }
        private void BtnStartClick(object sender, RoutedEventArgs e)
        {
            var isShown = (_startMenu.Visibility == Visibility.Visible);
            HideAllPopups();
            if(!isShown)
                ShowStartMenu();
        }
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializeTaskbar();
        }
        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
        private void PopActivityManagersMouseLeave(object sender, MouseEventArgs e)
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
            var ac = new Activity
            {
                Name = "nameless",
                Description = "This is the description of the test activity - " + DateTime.Now
            };
            ac.Uri = "http://tempori.org/" + ac.Id;
            ac.Participants.Add(new User() { Email = " 	snielsen@itu.dk" });
            ac.Meta.Data = "added meta data";
            ac.Owner = _owner;
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

        private void BDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void BDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var droppedFilePaths =
                e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (droppedFilePaths == null) return;
            var fInfo = new FileInfo(droppedFilePaths[0]);
            _client.AddResource(fInfo, ((ActivityButton)sender).ActivityId);
        }
    }
    public enum RenderStyle
    {
        Glass,
        Plain
    }
}
