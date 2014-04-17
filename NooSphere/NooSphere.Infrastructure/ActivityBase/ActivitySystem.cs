using System.ComponentModel.Design;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.Context.Location;
using NooSphere.Infrastructure.Events;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Infrastructure.Web;
using NooSphere.Model;
using NooSphere.Model.Device;
using NooSphere.Model.Primitives;
using NooSphere.Model.Users;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Raven.Json.Linq;
using NooSphere.Model.Resources;
using NooSphere.Model.Notifications;


namespace NooSphere.Infrastructure.ActivityBase
{
    public class ActivitySystem : ActivityNode
    {
        DocumentStore _documentStore;
        public string DatabaseName { get; private set; }

        public ActivitySystem( DatabaseConfiguration databaseConfiguration,bool localCaching=true)
        {
            DatabaseName = databaseConfiguration.DatabaseName;

            LocalCaching = localCaching;

            InitializeEvents();

            Ip = Net.GetIp( IpType.All );

            Port = databaseConfiguration.Port;

            Tracker = new LocationTracker(Ip);

            InitializeDocumentStore(Net.GetUrl(databaseConfiguration.Address, databaseConfiguration.Port, "").ToString());
        }

        ~ActivitySystem()
        {
            StopLocationTracker();
        }

        #region Privat Methods

        void InitializeDocumentStore( string address )
        {
            try
            {
                _documentStore = new DocumentStore
                {
                    Conventions =
                    {
                        FindTypeTagName = type =>
                        {
                            if (typeof(IUser).IsAssignableFrom(type))
                                return "IUser";
                            if (typeof(IActivity).IsAssignableFrom(type))
                                return "IActivity";
                            if (typeof(IDevice).IsAssignableFrom(type))
                                return "IDevice";
                            if (typeof(IResource).IsAssignableFrom(type))
                                return "IResource";
                            if (typeof(INotification).IsAssignableFrom(type))
                                return "INotification";
                            return DocumentConvention.DefaultTypeTagName(type);
                        }
                    }
                };

                _documentStore.ParseConnectionString("Url = " + address);
                _documentStore.Initialize();

                LoadStore();
                SubscribeToChanges();
                OnConnectionEstablished();
            }
            catch
            {
                throw new Exception("RavenDB data bases " + DatabaseName + "_is not running or not found on url "+address);
            }

        }

        void SubscribeToChanges()
        {
            _documentStore.Changes(DatabaseName).ForAllDocuments()
                          .Subscribe( change =>
                          {
                              using (var session = _documentStore.OpenSession(DatabaseName))
                              {
                                  var obj = session.Load<object>( change.Id );

                                  if (obj is IUser)
                                      HandleIUserMessages(change);
                                  else if (obj is IActivity)
                                      HandleIActivityMessages(change);
                                  else if (obj is IDevice)
                                      HandleIDeviceMessages(change);
                                  else if (obj is IResource)
                                      HandleIResourceMessages(change);
                                  else if (obj is INotification)
                                      HandleINotificationMessages(change);
                                  else // in case of delete message we cannot resolve the object
                                      HandleUnknownMessage(change);
                              }
                          } );
        }

        void HandleUnknownMessage( DocumentChangeNotification change )
        {
            if ( change.Type == DocumentChangeTypes.Delete )
            {
                if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : activitiesCache.ContainsKey(change.Id))
                    OnActivityRemoved( new ActivityRemovedEventArgs( change.Id ) );
                if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : usersCache.ContainsKey(change.Id))
                    OnUserRemoved( new UserRemovedEventArgs( change.Id ) ); //special as they are not kept in store, but in memory
                if ( devices.ContainsKey( change.Id ) )
                    OnDeviceRemoved( new DeviceRemovedEventArgs( change.Id ) );
                if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : resourcesCache.ContainsKey(change.Id))
                    OnResourceRemoved(new ResourceRemovedEventArgs(change.Id));
                if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : notificationsCache.ContainsKey(change.Id))
                    OnNotificationRemoved(new NotificationRemovedEventArgs(change.Id));
            }
        }

        void HandleIDeviceMessages( DocumentChangeNotification change )
        {
            switch ( change.Type )
            {
                case DocumentChangeTypes.Delete:
                {
                    OnDeviceRemoved( new DeviceRemovedEventArgs( change.Id ) );
                }
                    break;
                case DocumentChangeTypes.Put:
                {
                    using (var session = _documentStore.OpenSession(DatabaseName))
                    {
                        var device = session.Load<IDevice>( change.Id );
                        if ( devices.ContainsKey( change.Id ) )
                        {
                            OnDeviceChanged( new DeviceEventArgs( device ) );
                        }
                        else
                        {
                            OnDeviceAdded( new DeviceEventArgs( device ) );
                        }
                    }
                }
                    break;
                default:
                    Console.WriteLine( change.Type.ToString() + " received." );
                    break;
            }
        }

        void HandleIActivityMessages( DocumentChangeNotification change )
        {
            switch ( change.Type )
            {
                case DocumentChangeTypes.Delete:
                {
                    OnActivityRemoved( new ActivityRemovedEventArgs( change.Id ) );
                }
                    break;
                case DocumentChangeTypes.Put:
                {
                    using (var session = _documentStore.OpenSession(DatabaseName))
                    {
                        var activity = session.Load<IActivity>( change.Id );
                        if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : activitiesCache.ContainsKey(change.Id))
                        {
                            OnActivityChanged(new ActivityEventArgs( activity ));

                        }
                        else
                        {
                            OnActivityAdded( new ActivityEventArgs( activity ) );
                        }

                        if (!LocalCaching)
                            updatesInProgress.TryRemove(change.Id, out dump);
                    }
                }
                    break;
                default:
                    Console.WriteLine( change.Type.ToString() + " received." );
                    break;
            }
        }

        private INoo dump;

        void HandleIUserMessages( DocumentChangeNotification change )
        {
            switch ( change.Type )
            {
                case DocumentChangeTypes.Delete:
                {
                    OnUserRemoved( new UserRemovedEventArgs( change.Id ) );
                }
                    break;
                case DocumentChangeTypes.Put:
                {
                    using (var session = _documentStore.OpenSession(DatabaseName))
                    {
                        var user = session.Load<IUser>( change.Id );

                        if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : usersCache.ContainsKey(change.Id))
                        {
                            OnUserChanged( new UserEventArgs( user ) );
                        }
                        else
                        {
                            OnUserAdded( new UserEventArgs( user ) );
                        }

                        if (!LocalCaching)
                            updatesInProgress.TryRemove(change.Id,out dump);
                    }
                }
                    break;
                default:
                    Console.WriteLine( change.Type.ToString() + " received." );
                    break;
            }
        }

        void HandleIResourceMessages(DocumentChangeNotification change)
        {
            switch (change.Type)
            {
                case DocumentChangeTypes.Delete:
                    {
                        OnResourceRemoved(new ResourceRemovedEventArgs(change.Id));
                    }
                    break;
                case DocumentChangeTypes.Put:
                    {
                        using (var session = _documentStore.OpenSession("activitysystem"))
                        {
                            var resource = session.Load<IResource>(change.Id);
                            if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : resourcesCache.ContainsKey(change.Id))
                            {
                                OnResourceChanged(new ResourceEventArgs( resource ));
                            }
                            else
                            {
                                OnResourceAdded(new ResourceEventArgs(resource));
                            }

                            if (!LocalCaching)
                                updatesInProgress.TryRemove(change.Id, out dump);
                        }
                    }
                    break;
                default:
                    Console.WriteLine(change.Type.ToString() + " received.");
                    break;
            }
        }
        void HandleINotificationMessages(DocumentChangeNotification change)
        {
            switch (change.Type)
            {
                case DocumentChangeTypes.Delete:
                    {
                        OnNotificationRemoved(new NotificationRemovedEventArgs(change.Id));
                    }
                    break;
                case DocumentChangeTypes.Put:
                    {
                        using (var session = _documentStore.OpenSession("activitysystem"))
                        {
                            var not = session.Load<INotification>(change.Id);
                            if (!LocalCaching ? updatesInProgress.ContainsKey(change.Id) : notificationsCache.ContainsKey(change.Id))
                            {
                                OnNotificationChanged(new NotificationEventArgs(not));
                            }
                            else
                            {
                                OnNotificationAdded(new NotificationEventArgs(not));
                            }
                            if (!LocalCaching)
                                updatesInProgress.TryRemove(change.Id, out dump);
                        }
                    }
                    break;
                default:
                    Console.WriteLine(change.Type.ToString() + " received.");
                    break;
            }
        }

        void LoadStore()
        {
            if (!LocalCaching)
                return;

            using (var session = _documentStore.OpenSession(DatabaseName))
            {
                try
                {
                    var userResult = from user in session.Query<IUser>()
                                     select user;
                    foreach (var entry in userResult)
                    {
                        usersCache.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry != null ? entry : null);
                    }

                }
                catch (InvalidOperationException)
                {
                    HandleUnfoundType<IUser>();
                }

                try
                {
                    var activityResult = from activity in session.Query<IActivity>()
                                         select activity;

                    foreach (var entry in activityResult)
                    {
                        activitiesCache.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry);
                    }
                }
                catch (InvalidOperationException)
                {
                    HandleUnfoundType<Activity>();
                }

                try
                {
                    var deviceResult = from device in session.Query<IDevice>()
                                       where device.Type == typeof(IDevice).Name
                                       select device;

                foreach (var entry in deviceResult)
                {
                    devices.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry);
                }

                var resourceResult = from resource in session.Query<IResource>()
                                     select resource;

                foreach (var entry in resourceResult)
                {
                    resourcesCache.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry != null ? entry : null);
                }

                var notificationResult = from notification in session.Query<INotification>()
                                         select notification;

                foreach (var entry in notificationResult)
                {
                    notificationsCache.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry != null ? entry : null);
                }
		}
		catch (InvalidOperationException)
                {
                    HandleUnfoundType<Device>();
                }
            }
        }
	    private void HandleUnfoundType<T>()
	    {
	        if (!LocalCaching)
	            return;
            using (var session = _documentStore.OpenSession(DatabaseName))
            {
                var results = from result in session.Query<RavenJObject>()
                    select result;

                foreach (var entry in results)
                {
                    if (entry["Type"].ToString() == typeof (T).Name)
                    {
                        if (typeof (T).Name == "IUser")
                        {
                            Console.WriteLine("BackUp Convertion: Cannot find {0} and will convert to {1}", typeof(T).Name, typeof(User).Name);
                            var usr = entry.JsonDeserialization<User>();
                            usersCache.AddOrUpdate(usr.Id, usr, (key, oldValue) => usr);
                        }
                        if (typeof(T).Name == "IActivity")
                        {
                            Console.WriteLine("BackUp Convertion: Cannot find {0} and will convert to {1}", typeof(T).Name, typeof(Activity).Name);
                            var act = entry.JsonDeserialization<Activity>();
                            activitiesCache.AddOrUpdate(act.Id, act, (key, oldValue) => act);
                        }
                        if (typeof(T).Name == "IDevice")
                        {
                            Console.WriteLine("BackUp Convertion: Cannot find {0} and will convert to {1}", typeof(T).Name, typeof(Device).Name);
                            var dev = entry.JsonDeserialization<Device>();
                            devices.AddOrUpdate(dev.Id, dev, (key, oldValue) => dev);
                        }
                    }
                }
            }
        }
        public void UpdateAllProperties(object newUser)
        {
            foreach (var propertyInfo in newUser.GetType().GetProperties())
                if (propertyInfo.CanRead)
                {
                    var p = propertyInfo.GetValue(newUser, null);
                    var o = propertyInfo.GetValue(this, null);
                    if (o != p)
                    {
                        propertyInfo.SetValue(this, propertyInfo.GetValue(newUser, null));
                    }

                }
        }

        public void DeleteAllAttachments()
        {
            while (true)
            {
                var header = _documentStore.DatabaseCommands
                    .GetAttachmentHeadersStartingWith("", 0, 1)
                    .FirstOrDefault();
                if (header == null) return;
                _documentStore.DatabaseCommands.DeleteAttachment(header.Key, null);
            }
        }

        void AddToStore( INoo noo )
        {
            using (var session = _documentStore.OpenSession(DatabaseName))
            {
                session.Store( noo );
                session.SaveChanges();
            }
        }
        private Object updateLock = new Object();
        void UpdateStore( string id, INoo noo )
        {

            lock (updateLock)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var obj = session.Load<INoo>(id);
                    if (obj == null)
                        AddToStore(noo);
                    else
                    {
                        obj.UpdateAllProperties(noo);

                        if (!LocalCaching)
                            updatesInProgress.AddOrUpdate(id, noo, (key, oldValue) => noo);
                    }
                    session.SaveChanges();
                }
            }
            
        }

        void RemoveFromStore( string id )
        {
            using (var session = _documentStore.OpenSession(DatabaseName))
            {
                var obj = session.Load<INoo>( id );
                session.Delete( obj );
                session.SaveChanges();
            }
        }

        #endregion


        #region Public Methods

        private Object thisLock = new Object();
        public void AddFileResourceToActivity(Activity activity, Stream stream, string type, string filename)
        {
            var resource = new FileResource()
            {
                FileType = type,
                ActivityId = activity.Id,
                FileName = filename
            };

            lock (thisLock)
            {
                _documentStore.DatabaseCommands.PutAttachment(resource.Id,
                    null,
                    stream,
                    new RavenJObject
                        {
                            {"Extension", resource.FileType},
                        }
                    );

                if (type == "LOGO")
                    activity.Logo = resource;
                else
                {
                    activity.FileResources.Add(resource);
                    OnFileResourceAdded(new FileResourceEventArgs(resource));
                }
                UpdateActivity(activity);
            }
            stream.Dispose();
        }

        public void DeleteFileResource(FileResource resource)
        {
            _documentStore.DatabaseCommands.DeleteAttachment(resource.Id, null);

            OnFileResourceRemoved(new FileResourceRemovedEventArgs(resource.Id));
        }

        public Stream GetStreamFromFileResource(string resourceId)
        {
            try
            {
                var attachment = _documentStore.DatabaseCommands.GetAttachment(resourceId);
                if (attachment == null)
                    throw new FileNotFoundException("Resource not found in file store");
                return attachment.Data();
            }
            catch (FileNotFoundException ex)
            {

                return null;
            }

        }
        public void Run( string storeAddress )
        {
            InitializeDocumentStore( storeAddress );
        }
        public void Run(WebConfiguration configuration)
        {
            InitializeDocumentStore(Net.GetUrl(configuration.Address, configuration.Port, "").ToString());
        }

        public void StartLocationTracker()
        {
            if ( Tracker.IsRunning ) return;
            Tracker.Start();
        }

        public void StopLocationTracker()
        {
            if ( Tracker.IsRunning )
                Tracker.Stop();
        }

        public IUser FindUserByCid( string cid )
        {
            using (var session = _documentStore.OpenSession(DatabaseName))
            {
                var results = from user in session.Query<IUser>()
                              where user.Cid == cid
                              select user;
                var resultList = results.ToList();
                return resultList.Count > 0 ? resultList[ 0 ] : null;
            }
        }

        public override void AddUser( IUser user )
        {
            AddToStore( user );
        }

        public override void RemoveUser( string id )
        {
            RemoveFromStore( id );

            if(!LocalCaching)
                OnUserRemoved(new UserRemovedEventArgs(id));
        }

        public override void UpdateUser( IUser user )
        {
            UpdateStore( user.Id, user );
        }

        public override IUser GetUser( string id )
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from user in session.Query<IUser>()
                                  where user.Id == id
                                  select user;
                    var resultList = results.ToList();
                    return resultList.Count > 0 ? resultList[0] : null;
                }
            }
            else 
                return usersCache[ id ];
        }

        public override void AddResource(IResource res)
        {
            AddToStore(res);
        }

        public override void RemoveResource(string id)
        {
            RemoveFromStore(id);

            if(!LocalCaching)
                OnResourceRemoved(new ResourceRemovedEventArgs(id));
        }

        public override void UpdateResource(IResource res)
        {
            UpdateStore(res.Id, res);
        }

        public override IResource GetResource(string id)
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from res in session.Query<IResource>()
                                  where res.Id == id
                                  select res;
                    var resultList = results.ToList();
                    return resultList.Count > 0 ? resultList[0] : null;
                }
            }
            else
                return resourcesCache[id];
        }

        public override void AddActivity( IActivity act )
        {
            AddToStore( act );
        }

        public override void UpdateActivity( IActivity act )
        {
            UpdateStore( act.Id, act );
        }

        public override void RemoveActivity( string id )
        {
            RemoveFromStore( id );

            if(!LocalCaching)
                OnActivityRemoved(new ActivityRemovedEventArgs(id));
        }

        public override IActivity GetActivity( string id )
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from act in session.Query<IActivity>()
                                  where act.Id == id
                                  select act;
                    var resultList = results.ToList();
                    return resultList.Count > 0 ? resultList[0] : null;
                }
            }
            else
                return activitiesCache[ id ];
        }

        public override List<IActivity> GetActivities()
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var userResult = from act in session.Query<IActivity>()
                                     select act;
                    return userResult.ToList();
                }
            }
            else
                return activitiesCache.Values.ToList();
        }

        public override void AddDevice( IDevice dev )
        {
            OnDeviceAdded( new DeviceEventArgs( dev ) );
        }

        public override void UpdateDevice( IDevice dev )
        {
            OnDeviceChanged( new DeviceEventArgs( dev ) );
        }

        public override void RemoveDevice( string id )
        {
            OnDeviceRemoved( new DeviceRemovedEventArgs( id ) );
        }

        public override IDevice GetDevice( string id )
        {
            return devices[ id ];
        }

        public override List<IUser> GetUsers()
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from user in session.Query<IUser>()
                                  select user;
                    return results.ToList();
                }
            }
            else
                return usersCache.Values.ToList();
        }

        public override List<IDevice> GetDevices()
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from dev in session.Query<IDevice>()
                                  select dev;
                    return results.ToList();
                }
            }
            else
                return devices.Values.ToList();
        }

        public override List<IResource> GetResources()
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from res in session.Query<IResource>()
                                  select res;
                    return results.ToList();
                }
            }
            else
                return resourcesCache.Values.ToList();
        }

        public override void AddNotification(INotification n)
        {
            AddToStore(n);
        }

        public override void UpdateNotification(INotification n)
        {
            UpdateStore(n.Id, n);
        }

        public override void RemoveNotification(string id)
        {
            RemoveFromStore(id);

            if(!LocalCaching)
                OnNotificationRemoved(new NotificationRemovedEventArgs(id));
        }

        public override INotification GetNotification(string id)
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from not in session.Query<INotification>()
                                  where not.Id == id
                                  select not;
                    var resultList = results.ToList();
                    return resultList.Count > 0 ? resultList[0] : null;
                }
            }
            else
                return notificationsCache[id];
        }

        public override List<INotification> GetNotifications()
        {
            if (!LocalCaching)
            {
                using (var session = _documentStore.OpenSession(DatabaseName))
                {
                    var results = from not in session.Query<INotification>()
                                     select not;
                    return results.ToList();
                }
            }
            else
                return notificationsCache.Values.ToList();
        }

        #endregion
	        
        internal void RemoveDeviceByConnectionId(string connectionId)
        {
            IDevice device = null;

            foreach (var d in Devices.Values)
            {
                if (d.ConnectionId == connectionId)
                    device =d ;

            }
            if(device==null)return;
            RemoveDevice(device.Id);
                 
        }

        internal void HandleMessage(string obj)
        {
            var content = JsonConvert.DeserializeObject<JObject>(obj);
            var eventType = content["Event"].ToString();
            var data = content["Data"].ToString();

            switch ((NotificationType)Enum.Parse(typeof(NotificationType), eventType))
            {
                case NotificationType.Message:
                    OnMessageReceived(
                        new MessageEventArgs(Json.ConvertFromTypedJson<NooMessage>(data)));
                    break;
            }
        }
    }


    #region Extension Methods

    public static class ExtensionMethods
    {
        public static Collection<T> Remove<T>(
            this Collection<T> coll, Func<T, bool> condition )
        {
            var itemsToRemove = coll.Where( condition ).ToList();

            foreach ( var itemToRemove in itemsToRemove )
            {
                coll.Remove( itemToRemove );
            }

            return coll;
        }

        public static bool Contains<T>(
            this Collection<T> coll, Func<T, bool> condition )
        {
            if ( coll == null ) throw new ArgumentNullException( "coll" );
            if ( condition == null ) throw new ArgumentNullException( "condition" );
            var contains = coll.Where( condition ).ToList();
            return contains.Count > 0;
        }

        public static int FindIndex<T>( this IEnumerable<T> items, Func<T, bool> predicate )
        {
            if ( items == null ) throw new ArgumentNullException( "items" );
            if ( predicate == null ) throw new ArgumentNullException( "predicate" );

            int retVal = 0;
            foreach ( var item in items )
            {
                if ( predicate( item ) ) return retVal;
                retVal++;
            }
            return -1;
        }
    }

    #endregion
}