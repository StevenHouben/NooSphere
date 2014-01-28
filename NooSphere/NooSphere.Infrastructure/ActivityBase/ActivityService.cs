using System;
using System.Threading;
using System.Web.Http.Results;
using NooSphere.Infrastructure.Discovery;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Infrastructure.Web;
using NooSphere.Infrastructure.Events;
using NooSphere.Model.Device;


namespace NooSphere.Infrastructure.ActivityBase
{
    public class ActivityService
    {
        public event InitializedHandler Initialized = delegate { };
        public event ConnectionEstablishedHandler ConnectionEstablished = delegate { };

        public static ActivitySystem ActivitySystem;

        public static ActivityService Instance;

        WebApiServer _webApi;

        readonly BroadcastService _broadcast = new BroadcastService();

        public bool IsRunning
        {
            get { return _webApi != null && _webApi.IsRunning; }
        }

        public string Ip { get; private set; }
        public int Port { get; private set; }

        public ActivityService( ActivitySystem system, string ip, int port )
        {
            InitializeSevice( system, ip, port );

            ActivitySystem = system;

            ActivitySystem.ActivityAdded += ActivitySystem_ActivityAdded;
            ActivitySystem.ActivityChanged += ActivitySystem_ActivityChanged;
            ActivitySystem.ActivityRemoved += ActivitySystem_ActivityRemoved;

            ActivitySystem.DeviceAdded += ActivitySystem_DeviceAdded;
            ActivitySystem.DeviceChanged += ActivitySystem_DeviceChanged;
            ActivitySystem.DeviceRemoved += ActivitySystem_DeviceRemoved;

            ActivitySystem.UserAdded += ActivitySystem_UserAdded;
            ActivitySystem.UserChanged += ActivitySystem_UserChanged;
            ActivitySystem.UserRemoved += ActivitySystem_UserRemoved;

            ActivitySystem.ResourceAdded += ActivitySystem_ResourceAdded;
            ActivitySystem.ResourceChanged += ActivitySystem_ResourceChanged;
            ActivitySystem.ResourceRemoved += ActivitySystem_ResourceRemoved;

            Instance = this;

        }


        public virtual void StartBroadcast(DiscoveryType type, string hostName, string location = "undefined", string code = "-1")
        {
            var t = new Thread(() =>
            {
                StopBroadcast();
                _broadcast.Start(type, hostName, location, code,
                                  Net.GetUrl(Ip, Port, ""));
            }) { IsBackground = true };
            t.Start();
        }

        public virtual void StopBroadcast()
        {
            if (_broadcast != null)
                if (_broadcast.IsRunning)
                    _broadcast.Stop();
        }

        public void SendMessage(MessageType msgType, object body)
        {
            var message = new NooMessage()
            {
                Content = body,
                Type = msgType
            };

            Notifier.NotifyAll(NotificationType.Message, message);
        }

        public void SendMessage(Device device,MessageType msgType, object body)
        {
            var message = new NooMessage()
            {
                Content = body,
                Type = msgType
            };

            Notifier.NotifyConnection(device.ConnectionId, NotificationType.Message, message);
        }

        void ActivitySystem_ResourceRemoved(object sender, ResourceRemovedEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ResoureRemoved, e.Id);
        }

        void ActivitySystem_ResourceChanged(object sender, ResourceEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ResourceChanged, e.Resource);
        }

        void ActivitySystem_ResourceAdded(object sender, ResourceEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ResourceAdded, e.Resource);
        }

        void ActivitySystem_UserChanged(object sender, UserEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.UserChanged, e.User);
        }

        void ActivitySystem_UserRemoved(object sender, UserRemovedEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.UserRemoved, e.Id);
        }

        void ActivitySystem_UserAdded(object sender, UserEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.UserAdded, e.User);
        }

        void ActivitySystem_DeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.DeviceRemoved, e.Id);
        }

        void ActivitySystem_DeviceChanged(object sender, DeviceEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.DeviceChanged, e.Device);
        }

        void ActivitySystem_DeviceAdded(object sender, DeviceEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.DeviceAdded, e.Device);
        }

        void ActivitySystem_ActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ActivityRemoved, e.Id);
        }

        void ActivitySystem_ActivityChanged(object sender, ActivityEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ActivityChanged, e.Activity);
        }

        void ActivitySystem_ActivityAdded(object sender, ActivityEventArgs e)
        {
            Notifier.NotifyAll(NotificationType.ActivityAdded, e.Activity);
        }

        void InitializeSevice( ActivitySystem system, string ip, int port )
        {
            ActivitySystem = system;

            Ip = ip;
            Port = port;

            Initialized( this, new EventArgs() );
        }

        public void Start()
        {
            try
            {
                _webApi = new WebApiServer();
                _webApi.Start( Ip, Port );
                ConnectionEstablished( this, new EventArgs() );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
            }
        }

        public void Start( string ip, int port )
        {
            try
            {
                Ip = ip;
                Port = port;
                _webApi = new WebApiServer();
                _webApi.Start( Ip, Port );
                ConnectionEstablished( this, new EventArgs() );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
            }
        }

        public void Stop()
        {
            if ( IsRunning )
                _webApi.Stop();
        }
    }
}