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
using NooSphere.ActivitySystem.ActivityService.ActivityManagement;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.Client;
using NooSphere.ActivitySystem.Discovery;


namespace ActivityTablet
{

    public partial class Tablet : Window
    {
        #region Members

        private BasicClient client;
        private BasicHost host;
        private DiscoveryManager disc;

        private Dictionary<Guid, Activity> activities = new Dictionary<Guid, Activity>();
        private Dictionary<Guid, Button> proxies = new Dictionary<Guid,Button>();

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Tablet()
        {
            //Initializes design-time components
            InitializeComponent();

            Start();
        }
        private void Start()
        {
            disc = new DiscoveryManager();
            disc.Find();
            disc.DiscoveryAddressAdded += new DiscoveryAddressAddedHandler(disc_DiscoveryAddressAdded);
        }

        void disc_DiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Activity Manager found <" + e.ServicePair.Name + "> do you wish to connect to local activity service?",
                "Activity Manager Found", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes))
                StartClient(e.ServicePair.Address);
            else
                StartHost();
        }
        //1--- Start the Activitymanager
        private void StartHost()
        {
            Thread t = new Thread(() =>
            {
                host = new BasicHost();
                host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                host.Open(new ActivityManager(), typeof(IActivityManager),"tablet");

            });
            t.Start();
        }

        //2--- if host is launched, start the client
        void host_HostLaunched(object sender, EventArgs e)
        {
            StartClient();
        }



        //3--- Start theclient
        private void StartClient()
        {
            StartClient(host.Address);
        }
        private void StartClient(string addr)
        {
            client = new BasicClient(addr);
            client.Register();
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ActivityEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.ComEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.DeviceEvents);
            client.Subscribe(NooSphere.ActivitySystem.Contracts.NetEvents.EventType.FileEvents);

            // Set current participant on client
            client.CurrentParticipant = GetInitializedParticipant();

            client.ActivityAdded += new NooSphere.ActivitySystem.Client.Events.ActivityAddedHandler(client_ActivityAdded);
            client.ActivityRemoved += new NooSphere.ActivitySystem.Client.Events.ActivityRemovedHandler(client_ActivityRemoved);
            client.MessageReceived += new NooSphere.ActivitySystem.Client.Events.MessageReceivedHandler(client_MessageReceived);
            client.DeviceAdded += new NooSphere.ActivitySystem.Client.Events.DeviceAddedHandler(client_DeviceAdded);
            BuildUI();
        }
        #endregion




        #region Initializers
        /// <summary>
        /// Initializes the User Interface
        /// </summary>
        private void BuildUI()
        {
            foreach (Activity act in client.GetActivities().Values)
            {
                AddActivityUI(act);
            }
        }
        private void AddActivityUI(Activity activity)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                Button b = new Button();
                b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                b.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                b.Background = Brushes.Gray;
                b.Click += new RoutedEventHandler(b_Click);
                b.Width = 300;
                b.Height = 200;
                b.Content = activity.Identity.Name;
                panel.Children.Add(b);
                proxies.Add(activity.Identity.ID, b);
            }));
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        void GeneratedummyActivities()
        {
            for (int i = 0; i < 10; i++)
                client.AddActivity(GetInitializedActivity());
        }
        public Participant GetInitializedParticipant()
        {
            Participant p = new Participant();
            p.Email = "pitlabcloud@gmail.com";

            return p;
        }

        public Activity GetInitializedActivity()
        {
            Activity ac = new Activity();
            ac.Identity.Name = "test activity - " + DateTime.Now;
            ac.Identity.Description = "This is the description of the test activity - " + DateTime.Now;
            ac.Identity.Uri = "http://tempori.org/" + ac.Identity.ID;

            ac.Context = "random context model here";
            ac.Meta.Data = "added meta data";

            ac.Owner = GetInitializedParticipant();

            FileInfo fileInfo = new FileInfo(@"c:/test/sas.pdf");
            Resource res = new Resource(fileInfo);
            FileInfo fi1 = new FileInfo(@"c:/test/blablabla.txt");
            Resource res1 = new Resource(fi1);
            FileInfo fi2 = new FileInfo(@"c:/test/cool stuff.txt");
            Resource res2 = new Resource(fi2);
            FileInfo fi3 = new FileInfo(@"c:/test/great.txt");
            Resource res3 = new Resource(fi3);
            FileInfo fi4 = new FileInfo(@"c:/test/test.txt");
            Resource res4 = new Resource(fi4);

            NooSphere.Core.ActivityModel.Action act = new NooSphere.Core.ActivityModel.Action();
            act.Resources.Add(res);
            act.Resources.Add(res1);
            act.Resources.Add(res2);
            act.Resources.Add(res3);
            act.Resources.Add(res4);
            ac.Actions.Add(act);

            return ac;
        }

        #region NooSphere

       

        void client_ActivityAdded(object sender, NooSphere.ActivitySystem.Client.Events.ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
        }

        void client_DeviceAdded(object sender, NooSphere.ActivitySystem.Client.Events.DeviceEventArgs e)
        {
            MessageBox.Show(e.Device.Identity.ToString());
        }

        void client_MessageReceived(object sender, NooSphere.ActivitySystem.Client.Events.ComEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        void client_ActivityRemoved(object sender, NooSphere.ActivitySystem.Client.Events.ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.ID);
        }

        private void RemoveActivityUI(Guid guid)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                panel.Children.Remove(proxies[guid]);
            }));

            proxies.Remove(guid);
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
            client.AddActivity(GetInitializedActivity());
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.UnSubscribeAll();
        }
    }
}