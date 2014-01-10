using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ABC.Infrastructure.Context.Location;
using ABC.Infrastructure.Discovery;
using ABC.Infrastructure.Helpers;
using ABC.Model;
using ABC.Model.Device;
using ABC.Model.Users;


namespace ABC.Infrastructure.ActivityBase
{
    public abstract class ActivityController
    {
        #region Events

        public event UserAddedHandler UserAdded = delegate { };

        protected virtual void OnUserAdded( UserEventArgs e )
        {
            var handler = UserAdded;
            if ( handler != null ) handler( this, e );
        }

        public event UserRemovedHandler UserRemoved = delegate { };

        protected virtual void OnUserRemoved( UserRemovedEventArgs e )
        {
            var handler = UserRemoved;
            if ( handler != null ) handler( this, e );
        }

        public event UserChangedHandler UserChanged = delegate { };

        protected virtual void OnUserChanged( UserEventArgs e )
        {
            var handler = UserChanged;
            if ( handler != null ) handler( this, e );
        }

        public event ActivityAddedHandler ActivityAdded = delegate { };

        protected virtual void OnActivityAdded( ActivityEventArgs e )
        {
            var handler = ActivityAdded;
            if ( handler != null ) handler( this, e );
        }

        public event ActivityChangedHandler ActivityChanged = delegate { };

        protected virtual void OnActivityChanged( ActivityEventArgs e )
        {
            var handler = ActivityChanged;
            if ( handler != null ) handler( this, e );
        }

        public event ActivityRemovedHandler ActivityRemoved = delegate { };

        protected virtual void OnActivityRemoved( ActivityRemovedEventArgs e )
        {
            var handler = ActivityRemoved;
            if ( handler != null ) handler( this, e );
        }

        public event DeviceAddedHandler DeviceAdded = delegate { };

        protected virtual void OnDeviceAdded( DeviceEventArgs e )
        {
            var handler = DeviceAdded;
            if ( handler != null ) handler( this, e );
        }

        public event DeviceChangedHandler DeviceChanged = delegate { };

        protected virtual void OnDeviceChanged( DeviceEventArgs e )
        {
            var handler = DeviceChanged;
            if ( handler != null ) handler( this, e );
        }

        public event DeviceRemovedHandler DeviceRemoved = delegate { };

        protected virtual void OnDeviceRemoved( DeviceRemovedEventArgs e )
        {
            var handler = DeviceRemoved;
            if ( handler != null ) handler( this, e );
        }

        public event ConnectionEstablishedHandler ConnectionEstablished = delegate { };

        protected virtual void OnConnectionEstablished()
        {
            var handler = ConnectionEstablished;
            if ( handler != null ) handler( this, EventArgs.Empty );
        }

        #endregion


        #region Properties

        public string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public IDevice Device { get; set; }

        public Dictionary<string, IActivity> Activities
        {
            get { return new Dictionary<string, IActivity>( activities ); }
        }

        public Dictionary<string, IUser> Users
        {
            get { return new Dictionary<string, IUser>( users ); }
        }

        public Dictionary<string, IDevice> Devices
        {
            get { return new Dictionary<string, IDevice>( devices ); }
        }

        public LocationTracker Tracker { get; set; }

        #endregion


        #region Members

        readonly BroadcastService _broadcast = new BroadcastService();
        protected readonly ConcurrentDictionary<string, IUser> users = new ConcurrentDictionary<string, IUser>();
        protected readonly ConcurrentDictionary<string, IActivity> activities = new ConcurrentDictionary<string, IActivity>();
        protected readonly ConcurrentDictionary<string, IDevice> devices = new ConcurrentDictionary<string, IDevice>();

        #endregion


        #region Methods

        public virtual void StartBroadcast( DiscoveryType type, string hostName, string location = "undefined", string code = "-1" )
        {
            var t = new Thread( () =>
            {
                StopBroadcast();
                _broadcast.Start( type, hostName, location, code,
                                  Net.GetUrl( Ip, Port, "" ) );
            } ) { IsBackground = true };
            t.Start();
        }

        public virtual void StopBroadcast()
        {
            if ( _broadcast != null )
                if ( _broadcast.IsRunning )
                    _broadcast.Stop();
        }

        #endregion


        #region Constructor

        protected ActivityController()
        {
            ActivityAdded += ActivityNode_ActivityAdded;
            ActivityChanged += ActivityNode_ActivityChanged;
            ActivityRemoved += ActivityNode_ActivityRemoved;
            UserAdded += ActivityNode_UserAdded;
            UserRemoved += ActivityNode_UserRemoved;
            UserChanged += ActivityNode_UserChanged;
            DeviceAdded += ActivityNode_DeviceAdded;
            DeviceChanged += ActivityNode_DeviceChanged;
            DeviceRemoved += ActivityNode_DeviceRemoved;
        }

        #endregion


        #region Internal Handlers

        void ActivityNode_DeviceRemoved( object sender, DeviceRemovedEventArgs e )
        {
            IDevice backupDevice;
            devices.TryRemove( e.Id, out backupDevice );
        }

        void ActivityNode_DeviceChanged( object sender, DeviceEventArgs e )
        {
            devices[ e.Device.Id ].UpdateAllProperties( e.Device );
        }

        void ActivityNode_DeviceAdded( object sender, DeviceEventArgs e )
        {
            devices.AddOrUpdate( e.Device.Id, e.Device, ( key, oldValue ) => e.Device );
        }

        void ActivityNode_UserChanged( object sender, UserEventArgs e )
        {
            users[ e.User.Id ].UpdateAllProperties( e.User );
        }

        void ActivityNode_UserRemoved( object sender, UserRemovedEventArgs e )
        {
            IUser backupUser;
            users.TryRemove( e.Id, out backupUser );
        }

        void ActivityNode_UserAdded( object sender, UserEventArgs e )
        {
            users.AddOrUpdate( e.User.Id, e.User, ( key, oldValue ) => e.User );
        }

        void ActivityNode_ActivityRemoved( object sender, ActivityRemovedEventArgs e )
        {
            IActivity backupActivity;
            activities.TryRemove( e.Id, out backupActivity );
        }

        void ActivityNode_ActivityChanged( object sender, ActivityEventArgs e )
        {
            activities[ e.Activity.Id ].UpdateAllProperties( e.Activity );
        }

        void ActivityNode_ActivityAdded( object sender, ActivityEventArgs e )
        {
            activities.AddOrUpdate( e.Activity.Id, e.Activity, ( key, oldValue ) => e.Activity );
        }

        #endregion


        #region Abstract Methods

        public abstract void AddActivity( IActivity activity );
        public abstract void AddUser( IUser user );
        public abstract void RemoveUser( string id );
        public abstract void UpdateUser( IUser user );
        public abstract IUser GetUser( string id );
        public abstract List<IUser> GetUsers();
        public abstract void UpdateActivity( IActivity act );
        public abstract void RemoveActivity( string id );
        public abstract IActivity GetActivity( string id );
        public abstract List<IActivity> GetActivities();
        public abstract void AddDevice( IDevice dev );
        public abstract void UpdateDevice( IDevice dev );
        public abstract void RemoveDevice( string id );
        public abstract IDevice GetDevice( string id );
        public abstract List<IDevice> GetDevices();

        #endregion
    }
}