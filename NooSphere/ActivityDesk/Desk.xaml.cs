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
using System.Windows;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using ActivityDesk.Windowing;
using ActivityDesk.Visualizer;

using System.Threading;
using ActivityDesk.Visualizer.Definitions;
using System.Windows.Threading;
using System.Collections.Generic;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Core.Devices;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NooSphere.ActivitySystem.Base.Client;

namespace ActivityDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Desk : SurfaceWindow
    {
        #region Members

        private ActivityClient _client;
        private GenericHost _host;
        private DiscoveryManager _disc;
        private readonly User _user;
        private readonly Device _device;
        private DeskState _deskState;

        private Thread _discoveryThread;

        private Dictionary<Guid, SurfaceButton> _proxies = new Dictionary<Guid,SurfaceButton>();

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Desk()
        {
            //this._user = user;
            //Initializes design-time components
            InitializeComponent();

            //Disable default touch visualizations
            TouchVisualizer.SetShowsVisualizations(this, false);

            // Add handlers for window availability events.
            AddWindowAvailabilityHandlers();

            //Initializes tag definitions
            InitializeTags();

            SetDeskState(DeskState.Ready);

            _device = new Device()
            {
                DevicePortability = DevicePortability.Stationary,
                DeviceRole = DeviceRole.Mediator,
                DeviceType = DeviceType.Tabletop,
                Name = "Surface"
            };
        }

        public BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        private void SetDeskState(ActivityDesk.DeskState deskState)
        {
            this._deskState = deskState;

            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                switch (deskState)
                {
                    case ActivityDesk.DeskState.Active:
                        this.Background = (ImageBrush)this.Resources["green"];
                        break;
                    case ActivityDesk.DeskState.Locked:
                        this.Background = (ImageBrush)this.Resources["red"];
                        break;
                    case ActivityDesk.DeskState.Occupied:
                        this.Background = (ImageBrush)this.Resources["yellow"];
                        break;
                    case ActivityDesk.DeskState.Ready:
                        this.Background = (ImageBrush)this.Resources["blue"];
                        break;
                }
            }));
        }
        #endregion

        #region Initializers
        /// <summary>
        /// Initializes the User Interface
        /// </summary>
        private void InitializeUI()
        {
            SetDeskState(ActivityDesk.DeskState.Occupied);
        }

        private void AddResourceWindow()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                view.Items.Add(new TableWindow());
            }));
        }
        private void AddActivityUI(Activity activity)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                SurfaceButton b = new SurfaceButton();
                b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                b.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                b.Background = System.Windows.Media.Brushes.Gray;
                b.Click += new RoutedEventHandler(b_Click);
                b.Width = 300;
                b.Height = Double.NaN;
                b.Content = activity.Name;
                view.Items.Add(b);
            }));
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            
        }
        /// <summary>
        /// Initializes the tag defintions
        /// </summary>
        private void InitializeTags()
        {
            TagVisualizationDefinition definition = new SmartPhoneDefinition();
            definition.Source = new Uri("Visualizer/Visualizations/SmartPhone.xaml", UriKind.Relative);
            definition.TagRemovedBehavior = TagRemovedBehavior.Disappear;
            definition.LostTagTimeout = 1000;

            TagVisualizationDefinition definition2 = new TabletDefinition();
            definition2.Source = new Uri("Visualizer/Visualizations/VisualizationTablet.xaml", UriKind.Relative);
            definition2.LostTagTimeout = 1000;

            Visualizer.Definitions.Add(definition);
            Visualizer.Definitions.Add(definition2);
        }
        #endregion

        private void RunDiscovery()
        {
            _discoveryThread = new Thread(() =>
            {
                _disc = new DiscoveryManager();
                _disc.DiscoveryAddressAdded += DiscDiscoveryAddressAdded;
                _disc.DiscoveryFinished += DiscDiscoveryFinished;
                _disc.Find(DiscoveryType.WSDiscovery);
            }) {IsBackground = true};
            _discoveryThread.Start();
        }

        void DiscDiscoveryFinished(object o, DiscoveryEventArgs e)
        {
            if (_disc.ActivityServices.Count == 0)
                if(Visualizer.ActiveVisualizations.Count ==0)
                    SetDeskState(DeskState.Ready);
                else
                    SetDeskState(DeskState.Locked);
        }

        void DiscDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            StartClient(e.ServiceInfo.Address);
        }


        #region NooSphere

        /// <summary>
        /// 1--- Start the Activitymanager
        /// </summary>
        void StartHost()
        {
            var t = new Thread(() =>
            {
                _host = new GenericHost();
                _host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                _host.Open(new ActivityManager(new User(),"c:/files/"), typeof(IActivityManager),"desk");

            });
            t.Start();
        }

        /// <summary>
        /// 2--- if host is launched, start the client
        /// </summary>
        void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient(_host.Address);
        }

        /// <summary>
        /// 3--- Start theclient
        /// </summary>
        void StartClient(string addr)
        {   
            _client = new ActivityClient(@"c:/abcdesk/", _device) { CurrentUser = new User() };

            _client.ActivityAdded += ClientActivityAdded;
            _client.ActivityRemoved += ClientActivityRemoved;

            _client.Open(addr);
            InitializeUI();
        }

        void ClientActivityAdded(object sender, ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
        }

        void client_DeviceAdded(object sender, DeviceEventArgs e)
        {
            MessageBox.Show(e.Device.Id.ToString());
        }

        void client_MessageReceived(object sender, ComEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.Id);
        }

        private void RemoveActivityUI(Guid guid)
        {

        }
        #endregion

        #region UI Events
        /// <summary>
        /// Here when a ScatterViewItem moves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScatterDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            // The ScatterViewItem moved, so synchronize visualizations appropriately.
            // Note that doing so is unnecessary when TagVisualizerEvents.Mode is set
            // to Auto.
            //TagVisualizerEvents.Synchronize();

            // It's not necessary to check "is auto-update active?" and only call
            // Synchronize() if it isn't, because the Synchronize() method is smart
            // enough not to do redundant checking when auto-synchronize is on.
        }
        #endregion

        #region Visualizations
        /// <summary>
        /// Here when a visualization enters another object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationEnter(object sender, TagVisualizationEnterLeaveEventArgs e)
        {
            //((BaseVisualization)e.Visualization).Enter();
        }

        /// <summary>
        /// Here when a visualization leaves another object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationLeave(object sender, TagVisualizationEnterLeaveEventArgs e)
        {
            //((BaseVisualization)e.Visualization).Leave();
        }

        #endregion

        #region Surface Window Handlers
        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events.
            RemoveWindowAvailabilityHandlers();
        }
        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events.
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events.
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
        #endregion

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddActivity_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Visualizer_VisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            if (Visualizer.ActiveVisualizations.Count == 1)
            {
                RunDiscovery();
                SetDeskState(ActivityDesk.DeskState.Active);
            }
        }

        private void Visualizer_VisualizationRemoved(object sender, TagVisualizerEventArgs e)
        {
            if (Visualizer.ActiveVisualizations.Count == 0)
            {
                SetDeskState(ActivityDesk.DeskState.Ready);
            }
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }


            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                view.Items.Clear();
            }));
        }

        private void Visualizer_VisualizationMoved(object sender, TagVisualizerEventArgs e)
        {

        }

        private void Visualizer_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeTracker();
        }
    }
}