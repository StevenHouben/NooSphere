using System;

using ABC.Infrastructure.Web;
using ABC.Infrastructure.Events;


namespace ABC.Infrastructure.ActivityBase
{
    public class ActivityService
    {
        public event InitializedHandler Initialized = delegate { };
        public event ConnectionEstablishedHandler ConnectionEstablished = delegate { };

        public static ActivitySystem ActivitySystem;

        WebApiServer _webApi;

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