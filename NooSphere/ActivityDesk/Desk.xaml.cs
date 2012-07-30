using System;
using System.Windows;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using ActivityDesk.Visualizer.Visualization;
using ActivityDesk.Windowing;
using ActivityDesk.Visualizer;

using System.Threading;

using ActivityDesk.Visualizer.Definitions;
using System.Windows.Threading;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.ActivityClient;
using NooSphere.ActivitySystem.Events;
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.ActivityManager;
using NooSphere.ActivitySystem.Discovery.Client;
using System.Windows.Controls;
using NooSphere.Core.Devices;

namespace ActivityDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Desk : SurfaceWindow
    {
        #region Members

        private Client client;
        private BasicHost host;
        private DiscoveryManager disc;
        private User user;
        private Device device;
        private DeskState DeskState;

        private bool tagsAreSupported;

        private Dictionary<Guid, SurfaceButton> proxies = new Dictionary<Guid,SurfaceButton>();

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Desk()
        {
            //Initializes design-time components
            InitializeComponent();

            //Disable default touch visualizations
            TouchVisualizer.SetShowsVisualizations(this, false);

            // Add handlers for window availability events.
            AddWindowAvailabilityHandlers();

            //Initializes tag definitions
            InitializeTags();

            // Check the hardware, and modify the UI based on the supported capabilities.
            tagsAreSupported = InteractiveSurface.PrimarySurfaceDevice.IsTagRecognitionSupported;

            SetDeskState(DeskState.Ready);

            device = new Device()
            {
                DevicePortability = DevicePortability.Stationary,
                DeviceRole = DeviceRole.Mediator,
                DeviceType = DeviceType.Tabletop,
                Name = "Surface"
            };
        }

        private void SetDeskState(ActivityDesk.DeskState deskState)
        {
            this.DeskState = deskState;

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
            foreach (Activity ac in client.GetActivities())
            {
                AddActivityUI(ac);
            }

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
                b.Background = Brushes.Gray;
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
            definition.LostTagTimeout = 500;

            TagVisualizationDefinition definition2 = new TabletDefinition();
            definition2.Source = new Uri("Visualizer/Visualizations/VisualizationTablet.xaml", UriKind.Relative);
            definition2.LostTagTimeout = 500;

            Visualizer.Definitions.Add(definition);
            Visualizer.Definitions.Add(definition2);
        }
        #endregion

        private void RunDiscovery()
        {

            Thread t = new Thread(() =>
            {
                disc = new DiscoveryManager();
                disc.Find();
                disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
                disc.DiscoveryFinished += new DiscoveryFinishedHander(disc_DiscoveryFinished);
            });
            t.IsBackground = true;
            t.Start();
        }

        void disc_DiscoveryFinished(object o, DiscoveryEventArgs e)
        {
            if (disc.ActivityServices.Count == 0)
                SetDeskState(ActivityDesk.DeskState.Locked);
        }

        void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            StartClient(e.ServiceInfo.Address);
        }


        #region NooSphere

        /// <summary>
        /// 1--- Start the Activitymanager
        /// </summary>
        void StartHost()
        {
            Thread t = new Thread(() =>
            {
                host = new BasicHost();
                host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                host.Open(new ActivityManager(new User()), typeof(IActivityManager),"desk");

            });
            t.Start();
        }

        /// <summary>
        /// 2--- if host is launched, start the client
        /// </summary>
        void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient(host.Address);
        }

        /// <summary>
        /// 3--- Start theclient
        /// </summary>
        void StartClient(string addr)
        {
            client = new Client(addr);
            client.Register(device);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);

            // Set current participant on client
            client.CurrentUser = this.user;

            client.ActivityAdded += new ActivityAddedHandler(client_ActivityAdded);
            client.ActivityRemoved += new ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new MessageReceivedHandler(client_MessageReceived);
            client.DeviceAdded += new DeviceAddedHandler(client_DeviceAdded);
            InitializeUI();
        }

        void client_ActivityAdded(object sender, ActivityEventArgs e)
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

        void client_ActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.ID);
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
            TagVisualizerEvents.Synchronize();

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
            RunDiscovery();
            SetDeskState(ActivityDesk.DeskState.Active);
        }

        private void Visualizer_VisualizationRemoved(object sender, TagVisualizerEventArgs e)
        {
            if(Visualizer.ActiveVisualizations.Count ==0)
                SetDeskState(ActivityDesk.DeskState.Ready);
            if (client != null)
            {
                client.UnSubscribeAll();
                client.Unregister();
                client = null;
            }

            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                view.Items.Clear();
            }));
        }
    }
}