using ABC.Infrastructure.Context.Location;
using ABC.Infrastructure.Helpers;
using ABC.Model;
using ABC.Model.Device;
using ABC.Model.Primitives;
using ABC.Model.Users;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace ABC.Infrastructure.ActivityBase
{
    public class ActivitySystem : ActivityController
    {
        #region Members

        DocumentStore _documentStore;

        #endregion


        #region Constructor

        public ActivitySystem( string systemName = "activitysystem" )
        {

            Name = systemName;
            Ip = Net.GetIp( IpType.All );
            Port = 8000;
            Tracker = new LocationTracker(Ip);
        }

        ~ActivitySystem()
        {
            StopBroadcast();
            StopLocationTracker();
        }

        #endregion


        #region Eventhandlers

        void TrackerTagButtonDataReceived( Tag tag, TagEventArgs e )
        {
            var col = new Collection<IUser>( users.Values.ToList() );
            if ( col.Contains( u => u.Tag == e.Tag.Id ) )
            {
                int index = col.FindIndex( u => u.Tag == e.Tag.Id );

                if ( e.Tag.ButtonA == ButtonState.Pressed )
                {
                    users[ col[ index ].Id ].State = 2;
                    users[ col[ index ].Id ].Selected = true;
                }
                else if ( e.Tag.ButtonB == ButtonState.Pressed )
                {
                    users[ col[ index ].Id ].State = 1;
                    users[ col[ index ].Id ].Selected = true;
                }
                else
                {
                    users[ col[ index ].Id ].State = 0;
                    users[ col[ index ].Id ].Selected = true;
                }
                OnUserChanged( new UserEventArgs( users[ col[ index ].Id ] ) );
            }
        }

        void tracker_Detection( Detector detector, DetectionEventArgs e ) {}

        #endregion


        #region Privat Methods

        void InitializeDocumentStore( string address )
        {
            _documentStore = new DocumentStore
            {
                Conventions =
                {
                    FindTypeTagName = type =>
                    {
                        if ( typeof( IUser ).IsAssignableFrom( type ) )
                            return "IUser";
                        if ( typeof( IActivity ).IsAssignableFrom( type ) )
                            return "IActivity";
                        if ( typeof( IDevice ).IsAssignableFrom( type ) )
                            return "IDevice";
                        return DocumentConvention.DefaultTypeTagName( type );
                    }
                }
            };

            _documentStore.ParseConnectionString( "Url = " + address );
            _documentStore.Initialize();

            LoadStore();
            SubscribeToChanges();
            OnConnectionEstablished();
        }

        public T Cast<T>( object input )
        {
            return (T)input;
        }

        void SubscribeToChanges()
        {
            _documentStore.Changes( "activitysystem" ).ForAllDocuments()
                          .Subscribe( change =>
                          {
                              using ( var session = _documentStore.OpenSession( "activitysystem" ) )
                              {
                                  var obj = session.Load<object>( change.Id );
                                  if ( obj is IUser )
                                      HandleIUserMessages( change );
                                  else if ( obj is IActivity )
                                      HandleActivityMessages( change );
                                  else if ( obj is IDevice )
                                      HandleDeviceMessages( change );
                                  else
                                      HandleUnknowMessage( change );
                              }
                          } );
        }

        void HandleUnknowMessage( DocumentChangeNotification change )
        {
            if ( change.Type == DocumentChangeTypes.Delete )
            {
                if ( activities.ContainsKey( change.Id ) )
                    OnActivityRemoved( new ActivityRemovedEventArgs( change.Id ) );
                if ( users.ContainsKey( change.Id ) )
                    OnUserRemoved( new UserRemovedEventArgs( change.Id ) );
                if ( devices.ContainsKey( change.Id ) )
                    OnDeviceRemoved( new DeviceRemovedEventArgs( change.Id ) );
            }
        }

        void HandleDeviceMessages( DocumentChangeNotification change )
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
                    using ( var session = _documentStore.OpenSession( "activitysystem" ) )
                    {
                        var device = session.Load<IDevice>( change.Id );
                        if ( devices.ContainsKey( change.Id ) )
                        {
                            OnDeviceChanged( new DeviceEventArgs( devices[ change.Id ] ) );
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

        void HandleActivityMessages( DocumentChangeNotification change )
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
                    using ( var session = _documentStore.OpenSession( "activitysystem" ) )
                    {
                        var activity = session.Load<IActivity>( change.Id );
                        if ( activities.ContainsKey( change.Id ) )
                        {
                            OnActivityChanged( new ActivityEventArgs( activities[ change.Id ] ) );
                        }
                        else
                        {
                            OnActivityAdded( new ActivityEventArgs( activity ) );
                        }
                    }
                }
                    break;
                default:
                    Console.WriteLine( change.Type.ToString() + " received." );
                    break;
            }
        }

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
                    using ( var session = _documentStore.OpenSession( "activitysystem" ) )
                    {
                        var user = session.Load<IUser>( change.Id );
                        if ( users.ContainsKey( change.Id ) )
                        {
                            OnUserChanged( new UserEventArgs( user ) );
                        }
                        else
                        {
                            OnUserAdded( new UserEventArgs( user ) );
                        }
                    }
                }
                    break;
                default:
                    Console.WriteLine( change.Type.ToString() + " received." );
                    break;
            }
        }

        void LoadStore()
        {
            using ( var session = _documentStore.OpenSession( "activitysystem" ) )
            {
                var userResult = from user in session.Query<IUser>()
                                 where user.BaseType == typeof( IUser ).Name
                                 select user;

                foreach ( var entry in userResult )
                {
                    users.AddOrUpdate( entry.Id, entry, ( key, oldValue ) => entry != null ? entry : null );
                }

                var activityResult = from activity in session.Query<IActivity>()
                                     where activity.BaseType == typeof( IActivity ).Name
                                     select activity;

                foreach ( var entry in activityResult )
                {
                    activities.AddOrUpdate( entry.Id, entry, ( key, oldValue ) => entry );
                }

                //var deviceResult = from device in session.Query<IDevice>()
                //                   where device.BaseType == typeof( IDevice).Name
                //                   select device;

                //foreach (var entry in deviceResult)
                //{
                //    devices.AddOrUpdate(entry.Id, entry, (key, oldValue) => entry);
                //}
            }
        }

        void AddToStore( INoo noo )
        {
            using ( var session = _documentStore.OpenSession( "activitysystem" ) )
            {
                session.Store( noo );
                session.SaveChanges();
            }
        }

        void UpdateStore( string id, INoo noo )
        {
            using ( var session = _documentStore.OpenSession( "activitysystem" ) )
            {
                var obj = session.Load<INoo>( id );
                obj.UpdateAllProperties( noo );
                session.SaveChanges();
            }
        }

        void RemoveFromStore( string id )
        {
            using ( var session = _documentStore.OpenSession( "activitysystem" ) )
            {
                var obj = session.Load<INoo>( id );
                session.Delete( obj );
                session.SaveChanges();
            }
        }

        #endregion


        #region Public Methods

        public void Run( string storeAddress )
        {
            InitializeDocumentStore( storeAddress );
        }

        public void StartLocationTracker()
        {
            if ( Tracker.IsRunning ) return;
            Tracker.Detection += tracker_Detection;
            Tracker.TagButtonDataReceived += TrackerTagButtonDataReceived;
            Tracker.Start();
        }

        public void StopLocationTracker()
        {
            if ( Tracker.IsRunning )
                Tracker.Stop();
        }

        public IUser FindUserByCid( string cid )
        {
            using ( var session = _documentStore.OpenSession( "activitysystem" ) )
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
        }

        public override void UpdateUser( IUser user )
        {
            UpdateStore( user.Id, user );
        }

        public override IUser GetUser( string id )
        {
            return users[ id ];
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
        }

        public override IActivity GetActivity( string id )
        {
            return activities[ id ];
        }

        public override List<IActivity> GetActivities()
        {
            return activities.Values.ToList();
        }

        public override void AddDevice( IDevice dev )
        {
            //AddToStore(dev);
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
            return users.Values.ToList();
        }

        public override List<IDevice> GetDevices()
        {
            return devices.Values.ToList();
        }

        #endregion
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