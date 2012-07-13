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
using NooSphere.ActivitySystem.ActivityService.ActivityManagement;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Client;
using NooSphere.ActivitySystem.Host;
using NooSphere.Platform.Windows.VDM;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery.Client;

using ActivityUI.Properties;
using ActivityUI.Login;

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
        private Dictionary<Guid, Button> buttons = new Dictionary<Guid, Button>();
        private Activity currentActivity;
        private Button currentButton;
        private LoginWindow login;
        private bool startingUp = true;
        #endregion

        #region Constructor
        public ActivityBar()
        {
            InitializeComponent();
            DisableUI();

            login = new LoginWindow();
            login.LoggedIn += new EventHandler(login_LoggedIn);
            login.Show();
        }

        void login_LoggedIn(object sender, EventArgs e)
        {
            login.Close();
            StartSystem();
        }

        private void StartSystem()
        {
            device = login.Device;
            device.Location = "pIT lab";
            owner = login.User;
            startMode = login.Mode;
            Start();
        }
        #endregion

        #region Private Members
        private void Start()
        {
            RunDiscovery();

            if (startMode == StartUpMode.Host)
                StartHost();
            else
            {
                chkBroadcast.IsChecked = chkBroadcast.IsEnabled = false;
                Settings.Default.CHECK_BROADCAST = false;
            }
        }
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
        private void StartHost()
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
        private void StartClient()
        {
            StartClient(host.Address);
        }
        private void StartClient(string addr)
        {
            client = new BasicClient(addr);

            client.Register();
            client.CurrentParticipant = owner;

            client.GetActivities();

            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);

            client.DeviceAdded += new NooSphere.ActivitySystem.Client.Events.DeviceAddedHandler(client_DeviceAdded);
            client.ActivityAdded += new NooSphere.ActivitySystem.Client.Events.ActivityAddedHandler(client_ActivityAdded);
            client.DeviceRemoved+=new NooSphere.ActivitySystem.Client.Events.DeviceRemovedHandler(client_DeviceRemoved);
            client.ActivityRemoved += new NooSphere.ActivitySystem.Client.Events.ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new NooSphere.ActivitySystem.Client.Events.MessageReceivedHandler(client_MessageReceived);

            BuildUI();

            startingUp = false;
        }
        private void DisableUI()
        {
            ToggleUI(false);
        }
        private void EnableUI()
        {
            ToggleUI(true);
        }
        private void ToggleUI(bool d)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                btnAdd.IsEnabled = d;
                btnHome.IsEnabled = d;
            }));
        }
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
        private void BuildDiscoveryUI()
        {
            txtDeviceName.Text = device.Name;
            txtEmail.Text = owner.Email;
            txtUsername.Text = owner.Name;
        }
        private void RemoveActivityUI(Guid id)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Body.Children.Remove(buttons[id]);
            }));
        }
        private void AddActivityUI(Activity activity)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Proxy prox = ConvertActivityToProxy(activity);

                Button b = new Button();
                b.Click += new RoutedEventHandler(b_Click);
                b.MouseDown += new MouseButtonEventHandler(b_MouseDown);
                b.Tag = prox;
                b.Width = 300;
                b.Height = this.Height - 5;
                b.Content = prox.Activity.Name;
                Body.Children.Add(b);

                buttons.Add(prox.Activity.Id, b);
            }));
        }
        private Proxy ConvertActivityToProxy(Activity activity)
        {
            Proxy p = new Proxy();
            p.Desktop = new VirtualDesktop();
            p.Activity = activity;
            VirtualDesktopManager.Desktops.Add(p.Desktop);
            return p;
        }
        private Proxy GetProxyFromButton(Button b)
        {
            return (Proxy)b.Tag;
        }
        private void ShowActivityButtonContextMenu(object sender)
        {
            currentButton = (Button)sender;
            popupActivity.PlacementTarget = currentButton;
            currentActivity = ((Proxy)currentButton.Tag).Activity;
            popupActivity.IsOpen = !popupActivity.IsOpen;
            txtName.Text = currentActivity.Name;
            foreach (string u in currentActivity.Participants.Values)
                txtParticipants.Text = u;

        }
        #endregion

        #region Event Handlers
        void b_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                ShowActivityButtonContextMenu(sender);
            }
        }
        private void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient();
        }
        private void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            if (startMode == StartUpMode.Client && startingUp)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Activity Manager found '" + e.ServicePair.Name + "' do you wish to connect to local activity service?",
               "Activity Manager Found", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                    StartClient(e.ServicePair.Address);
                else
                    StartHost();
            }
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                managerlist.Items.Add(e.ServicePair.Name + " at " + e.ServicePair.Address);
            }));

        }
        void client_DeviceRemoved(object sender, NooSphere.ActivitySystem.Client.Events.DeviceEventArgs e)
        {
            AddToLog("Device Removed\n");
        }
        private void client_DeviceAdded(object sender, NooSphere.ActivitySystem.Client.Events.DeviceEventArgs e)
        {
            AddToLog("Device Added\n");
        }

        private void AddToLog(string output)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                txtLog.Text = output + txtLog.Text;
            }));

        }
        private void client_MessageReceived(object sender, NooSphere.ActivitySystem.Client.Events.ComEventArgs e)
        {
            AddToLog(e.Message+"\n");
        }
        private void client_ActivityRemoved(object sender, NooSphere.ActivitySystem.Client.Events.ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.ID);
            AddToLog("Activity Added\n");
        }

        private void client_ActivityAdded(object obj, NooSphere.ActivitySystem.Client.Events.ActivityEventArgs e)
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
            try
            {
                VirtualDesktopManager.CurrentDesktop = ((Proxy)((Button)sender).Tag).Desktop;
            }
            catch
            {
                VirtualDesktopManager.UninitDesktops();
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            popActivityManagers.PlacementTarget = btnStart;
            popActivityManagers.IsOpen = !popActivityManagers.IsOpen;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get pointer to our own application
            IntPtr handle = new WindowInteropHelper(this).Handle;

            //Force glass style
            ApplyGlass(handle);

            NooSphere.Platform.Windows.Dock.AppBarFunctions.SetAppBar(this, NooSphere.Platform.Windows.Dock.AppBarPosition.Top);
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
            popActivityManagers.IsOpen = false;
            device.Name = txtDeviceName.Text;
            if (Settings.Default.CHECK_BROADCAST)
                host.StartBroadcast(device.Name, device.Location);
            RunDiscovery();
        }
        private void popupActivity_MouseLeave(object sender, MouseEventArgs e)
        {
            popupActivity.IsOpen = false;
            currentButton.Content = txtName.Text;
            currentActivity.Name = txtName.Text;
            client.UpdateActivity(currentActivity);
        }

        #endregion

        #region Helper

        public Activity GetInitializedActivity()
        {
            Activity ac = new Activity();
            ac.Name = "test activity - " + DateTime.Now;
            ac.Description = "This is the description of the test activity - " + DateTime.Now;
            ac.Uri = "http://tempori.org/" + ac.Id;

            ac.Context = "random context model here";
            ac.Meta.Data = "added meta data";

            ac.Participants.Add(owner.Id, "Owner");

            NooSphere.Core.ActivityModel.Action act = new NooSphere.Core.ActivityModel.Action();
            //act.Resources.Add(new Resource(new FileInfo(@"c:/test/sas.pdf")));
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

        private void btnDeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)popupActivity.PlacementTarget;
            Guid g = GetProxyFromButton(b).Activity.Id;
            client.RemoveActivity(g);
        }

        private void chkBroadcast_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.CHECK_BROADCAST = (bool)chkBroadcast.IsChecked;
            CheckBroadCast(Settings.Default.CHECK_BROADCAST);
        }
        private void CheckBroadCast(bool check)
        {
            if (check)
                host.StartBroadcast(device.Name, device.Location);
            else
                host.StopBroadcast();
        }

        private void btnMsg_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessage("hello world");
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtInput.Text);
        }

        private void SendMessage(string p)
        {
            client.SendMessage(p);
            txtInput.Text = "";
        }


    }
}
