using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NooSphere.Infrastructure.Context.Location;
using NooSphere.Model;
using NooSphere.Model.Device;
using NooSphere.Model.Users;
using NooSphere.Model.Resources;
using NooSphere.Model.Notifications;


namespace NooSphere.Infrastructure.ActivityBase
{
    public abstract class ActivityNode
    {
        #region Events

        public event UserAddedHandler UserAdded = delegate { };

        protected virtual void OnUserAdded(UserEventArgs e)
        {
            var handler = UserAdded;
            if (handler != null) handler(this, e);
        }

        public event UserRemovedHandler UserRemoved = delegate { };

        protected virtual void OnUserRemoved(UserRemovedEventArgs e)
        {
            var handler = UserRemoved;
            if (handler != null) handler(this, e);
        }

        public event UserChangedHandler UserChanged = delegate { };

        protected virtual void OnUserChanged(UserEventArgs e)
        {
            var handler = UserChanged;
            if (handler != null) handler(this, e);
        }

        public event ActivityAddedHandler ActivityAdded = delegate { };

        protected virtual void OnActivityAdded(ActivityEventArgs e)
        {
            var handler = ActivityAdded;
            if (handler != null) handler(this, e);
        }

        public event ActivityChangedHandler ActivityChanged = delegate { };

        protected virtual void OnActivityChanged(ActivityEventArgs e)
        {
            var handler = ActivityChanged;
            if (handler != null) handler(this, e);
        }

        public event ActivityRemovedHandler ActivityRemoved = delegate { };

        protected virtual void OnActivityRemoved(ActivityRemovedEventArgs e)
        {
            var handler = ActivityRemoved;
            if (handler != null) handler(this, e);
        }

        public event MessageReceivedHandler MessageReceived = delegate { };

        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            var handler = MessageReceived;
            if (handler != null) handler(this, e);
        }

        public event DeviceAddedHandler DeviceAdded = delegate { };

        protected virtual void OnDeviceAdded(DeviceEventArgs e)
        {
            var handler = DeviceAdded;
            if (handler != null) handler(this, e);
        }

        public event DeviceChangedHandler DeviceChanged = delegate { };

        protected virtual void OnDeviceChanged(DeviceEventArgs e)
        {
            var handler = DeviceChanged;
            if (handler != null) handler(this, e);
        }

        public event DeviceRemovedHandler DeviceRemoved = delegate { };

        protected virtual void OnDeviceRemoved(DeviceRemovedEventArgs e)
        {
            var handler = DeviceRemoved;
            if (handler != null) handler(this, e);
        }

        public event ResourceAddedHandler ResourceAdded = delegate { };

        protected virtual void OnResourceAdded(ResourceEventArgs e)
        {
            var handler = ResourceAdded;
            if (handler != null) handler(this, e);
        }

        public event ResourceRemovedHandler ResourceRemoved = delegate { };

        protected virtual void OnResourceRemoved(ResourceRemovedEventArgs e)
        {
            var handler = ResourceRemoved;
            if (handler != null) handler(this, e);
        }

        public event ResourceChangedHandler ResourceChanged = delegate { };

        protected virtual void OnResourceChanged(ResourceEventArgs e)
        {
            var handler = ResourceChanged;
            if (handler != null) handler(this, e);
        }
        public event NotificationAddedHandler NotificationAdded = delegate { };

        protected virtual void OnNotificationAdded(NotificationEventArgs e)
        {
            var handler = NotificationAdded;
            if (handler != null) handler(this, e);
        }

        public event NotificationRemovedHandler NotificationRemoved = delegate { };

        protected virtual void OnNotificationRemoved(NotificationRemovedEventArgs e)
        {
            var handler = NotificationRemoved;
            if (handler != null) handler(this, e);
        }

        public event NotificationChangedHandler NotificationChanged = delegate { };

        protected virtual void OnNotificationChanged(NotificationEventArgs e)
        {
            var handler = NotificationChanged;
            if (handler != null) handler(this, e);
        }

        public event ConnectionEstablishedHandler ConnectionEstablished = delegate { };

        protected virtual void OnConnectionEstablished()
        {
            var handler = ConnectionEstablished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event FileResourceAddedHandler FileResourceAdded = delegate {};
        protected virtual void OnFileResourceAdded(FileResourceEventArgs e)
        {
            var handler = FileResourceAdded;
            if (handler != null) handler(this, e);
        }

        public event FileResourceChangedHandler FileResourceChanged = delegate { };
        protected virtual void OnFileResourceChanged(FileResourceEventArgs e)
        {
            var handler = FileResourceChanged;
            if (handler != null) handler(this, e);
        }

        public event FileResourceRemovedHandler FileResourceRemoved = delegate { };
        protected virtual void OnFileResourceRemoved(FileResourceRemovedEventArgs e)
        {
            var handler = FileResourceRemoved;
            if (handler != null) handler(this, e);
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

        public Dictionary<string, IResource> Resources
        {
            get { return new Dictionary<string, IResource>(resources); }
        }

        public Dictionary<string, INotification> Notifications
        {
            get { return new Dictionary<string, INotification>(notifications); }
        }

        public LocationTracker Tracker { get; set; }

        #endregion


        #region Members

        protected readonly ConcurrentDictionary<string, IUser> users = new ConcurrentDictionary<string, IUser>();
        protected readonly ConcurrentDictionary<string, IActivity> activities = new ConcurrentDictionary<string, IActivity>();
        protected readonly ConcurrentDictionary<string, IDevice> devices = new ConcurrentDictionary<string, IDevice>();
        protected readonly ConcurrentDictionary<string, IResource> resources = new ConcurrentDictionary<string, IResource>();
        protected readonly ConcurrentDictionary<string, INotification> notifications = new ConcurrentDictionary<string, INotification>();

        #endregion


        #region Methods

      

        #endregion


        #region Constructor

        protected ActivityNode()
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
            ResourceAdded += ActivityNode_ResourceAdded;
            ResourceChanged += ActivityNode_ResourceChanged;
            ResourceRemoved += ActivityNode_ResourceRemoved;
            NotificationAdded += ActivityNode_NotificationAdded;
            NotificationChanged += ActivityNode_NotificationChanged;
            NotificationRemoved += ActivityNode_NotificationRemoved;
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

        void ActivityNode_ResourceChanged(object sender, ResourceEventArgs e)
        {
            resources[e.Resource.Id].UpdateAllProperties(e.Resource);
        }

        void ActivityNode_ResourceRemoved(object sender, ResourceRemovedEventArgs e)
        {
            IResource backupResource;
            resources.TryRemove(e.Id, out backupResource);
        }

        void ActivityNode_ResourceAdded(object sender, ResourceEventArgs e)
        {
            resources.AddOrUpdate(e.Resource.Id, e.Resource, (key, oldValue) => e.Resource);
        }
        void ActivityNode_NotificationChanged(object sender, NotificationEventArgs e)
        {
            notifications[e.Notification.Id].UpdateAllProperties(e.Notification);
        }

        void ActivityNode_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            INotification backupNotification;
            notifications.TryRemove(e.Id, out backupNotification);
        }

        void ActivityNode_NotificationAdded(object sender, NotificationEventArgs e)
        {
            notifications.AddOrUpdate(e.Notification.Id, e.Notification, (key, oldValue) => e.Notification);
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
        public abstract void AddResource(IResource resource);
        public abstract void RemoveResource(string id);
        public abstract void UpdateResource(IResource resource);
        public abstract IResource GetResource(string id);
        public abstract List<IResource> GetResources();
        public abstract void AddNotification(INotification notification);
        public abstract void RemoveNotification(string id);
        public abstract void UpdateNotification(INotification notification);
        public abstract INotification GetNotification(string id);
        public abstract List<INotification> GetNotifications();

        #endregion
    }
}