using System;
using System.Collections.Generic;
using System.IO;
using NooSphere.Infrastructure.Web;
using NooSphere.Model.Device;
using NooSphere.Model.Users;

using NooSphere.Infrastructure.Helpers;
using NooSphere.Model;

using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.Events;


namespace NooSphere.Infrastructure.ActivityBase
{
    public class ActivityClient : ActivityNode
    {
        #region Members

        readonly Connection _eventHandler;
        string Address { get; set; }
        bool _connected;

        #endregion

        #region Constructor/Destructor

        public ActivityClient( string ip, int port, IDevice device )
        {
            Ip = ip;
            Port = port;

            Address = Net.GetUrl( ip, port, "" ).ToString();

            Device = device;

            try
            {
                _eventHandler = new Connection(Address);
                _eventHandler.JsonSerializer.TypeNameHandling = TypeNameHandling.Objects;
                _eventHandler.Received += eventHandler_Received;
                _eventHandler.Start().Wait();
            }
            catch(HttpClientException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        ~ActivityClient()
        {
            if (_connected)
            {
                try
                {
                    _eventHandler.Stop();
                }
                catch (Exception)
                {
                    
                }

            }
        }

        #endregion


        #region Private Members

        void Initialize()
        {
            var acts = GetActivities();

            foreach (var item in acts)
                activities.AddOrUpdate(item.Id, item, (key, oldValue) => item);

            var usrs = GetUsers();
            foreach (var item in usrs)
                users.AddOrUpdate(item.Id, item, (key, oldValue) => item);

            var dvs = GetDevices();
            foreach (var item in dvs)
                devices.AddOrUpdate(item.Id, item, (key, oldValue) => item);

            Device.ConnectionId = _eventHandler.ConnectionId;
            AddDevice(Device);
        }

        void eventHandler_Received( string obj )
        {
            if ( obj == "Connected" )
            {
                _connected = true;
                Initialize();
                OnConnectionEstablished();
                return;
            }
            var content = JsonConvert.DeserializeObject<JObject>( obj );
            var eventType = content[ "Event" ].ToString();
            var data = content[ "Data" ].ToString();

            switch ( (NotificationType)Enum.Parse( typeof( NotificationType ), eventType ) )
            {
                case NotificationType.ActivityAdded:
                    OnActivityAdded( new ActivityEventArgs( Json.ConvertFromTypedJson<IActivity>( data ) ) );
                    break;
                case NotificationType.ActivityChanged:
                    OnActivityChanged( new ActivityEventArgs( Json.ConvertFromTypedJson<IActivity>( data ) ) );
                    break;
                case NotificationType.ActivityRemoved:
                    OnActivityRemoved(
                        new ActivityRemovedEventArgs(
                            JsonConvert.DeserializeObject<JObject>( data )[ "Id" ].ToString() ) );
                    break;
                case NotificationType.UserAdded:
                    OnUserAdded( new UserEventArgs( Json.ConvertFromTypedJson<IUser>( data ) ) );
                    break;
                case NotificationType.UserChanged:
                    OnUserChanged( new UserEventArgs( Json.ConvertFromTypedJson<IUser>( data ) ) );
                    break;
                case NotificationType.UserRemoved:
                    OnUserRemoved(
                        new UserRemovedEventArgs( data ) );
                    break;
                case NotificationType.ResourceAdded:
                    OnResourceAdded(
                            new ResourceEventArgs(Json.ConvertFromTypedJson<Resource>(data)));
                    break;
                case NotificationType.ResourceChanged:
                    OnResourceChanged(
                            new ResourceEventArgs(Json.ConvertFromTypedJson<Resource>(data)));
                    break;
                case NotificationType.ResoureRemoved:
                    OnResourceRemoved(
                            new ResourceRemovedEventArgs(data));
                    break;
                case NotificationType.Message:
                    OnMessageReceived(
                        new MessageEventArgs(Json.ConvertFromTypedJson<NooMessage>(data)));
                    break;
            }
        }

        #endregion


        #region Public Members


        public void SendMessage(MessageType type, object message)
        {
            var msg = new NooMessage()
            {
                Content = message,
                Type = type
            };

            var output = ConstructEvent(NotificationType.Message,msg);

            _eventHandler.Send(output);
        }
        protected object ConstructEvent(NotificationType type, object obj)
        {
            var notevent = new { Event = type.ToString(), Data = obj };
            return notevent;
        }

        public void AddResource(IActivity activity, MemoryStream stream)
        {
            Rest.UploadFile(Address + Url.Resources, activity.Id, stream);
        }

        public Stream GetResource(Resource resource)
        {
            return Rest.DownloadFile(Address + Url.Resources, resource.Id);
        }

        public override void AddActivity( IActivity activity )
        {
            Rest.Post( Address + Url.Activities, activity );
        }

        public override void AddUser( IUser user )
        {
            Rest.Post( Address + Url.Users, user );
        }

        public override void RemoveUser( string id )
        {
            Rest.Delete( Address + Url.Users, id );
        }

        public override void UpdateUser( IUser user )
        {
            //Rest.Put( Address + Url.Users, user );
        }

        public override IUser GetUser( string id )
        {
            return Json.ConvertFromTypedJson<IUser>( Rest.Get( Address + Url.Users, id ) );
        }

        public override void UpdateActivity( IActivity act )
        {
            Rest.Put( Address + Url.Activities, act );
        }

        public override void RemoveActivity( string id )
        {
            Rest.Delete( Address + Url.Activities, id );
        }

        public override IActivity GetActivity( string id )
        {
            return Json.ConvertFromTypedJson<IActivity>( Rest.Get( Address + Url.Activities, id ) );
        }

        public override List<IActivity> GetActivities()
        {
            return Json.ConvertFromTypedJson<List<IActivity>>(Rest.Get(Address + Url.Activities, ""));
        }

        public override void AddDevice( IDevice dev )
        {
            Rest.Post( Address + Url.Devices, dev );
        }

        public override void UpdateDevice( IDevice dev )
        {
            Rest.Put( Address + Url.Devices, dev );
        }

        public override void RemoveDevice( string id )
        {
            Rest.Delete( Address + Url.Devices, id );
        }

        public override IDevice GetDevice( string id )
        {
            return Json.ConvertFromTypedJson<IDevice>( Rest.Get( Address + Url.Devices, id ) );
        }

        public Type NotifierType { get; set; }

        public override List<IUser> GetUsers()
        {
            return Json.ConvertFromTypedJson<List<IUser>>(Rest.Get(Address + Url.Users, ""));
        }

        public override List<IDevice> GetDevices()
        {
            return Json.ConvertFromTypedJson<List<IDevice>>(Rest.Get(Address + Url.Devices, ""));
        }

        #endregion
    }

    public enum Url
    {
        Activities,
        Devices,
        Subscribers,
        Messages,
        Users,
        Resources
    }
}