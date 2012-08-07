/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel.Description;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Helpers;
using Newtonsoft.Json;

namespace NooSphere.ActivitySystem.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ActivityClient : NetEventHandler,IActivityNode
    {
        #region Events
        public event ConnectionEstablishedHandler ConnectionEstablished = null;
        public event InitializedHandler Initialized = null;
        #endregion

        #region Private Members
        private readonly Dictionary<Type, ServiceHost> _callbackServices = new Dictionary<Type, ServiceHost>();
        #endregion

        #region Properties
        public string Ip { get; set; }
        public string ClientName { get; set; }
        public string DeviceId { get; private set; }
        public string ServiceAddress { get; set; }
        public User CurrentUser { get; set; }
        public string LocalPath { get; private set; }
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address">The address of the service the client needs to connect to</param>
        /// <param name="localFileDirectory">The local file directory of the client</param>
        public ActivityClient(string address,string localFileDirectory)
        {
            Connect(address);
            LocalPath = localFileDirectory;
            OnInitializedEvent(new EventArgs());
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Tests the connection to the service
        /// </summary>
        /// <param name="addr">The address of the service</param>
        private void TestConnection(string addr)
        {
            Console.WriteLine("Activity Client Attempt to connect to {0}",addr);
            bool res;
            int attempts = 0;
            const int maxAttemps = 20;
            do
            {
                ServiceAddress = addr;
                res = JsonConvert.DeserializeObject<bool>(Rest.Get(ServiceAddress));
                Console.WriteLine("BasicClient: Service active? -> {0}", res);
                Thread.Sleep(100);
                attempts++;
            }
            while (res == false && attempts<maxAttemps);
            if (res)
                OnConnectionEstablishedEvent(new EventArgs());
            else
                throw new Exception("Could not connect to: " + addr);
        }

        /// <summary>
        /// Connects the client to the activity service
        /// </summary>
        /// <param name="address">The address of the service</param>
        private void Connect(string address)
        {
            Ip = Net.GetIp(IPType.All);
            TestConnection(address);
        }

        /// <summary>
        /// Starts a callback service. The activity manager uses this service to publish
        /// events.
        /// </summary>
        /// <param name="service">The type of callback service</param>
        /// <returns>The port of the deployed service</returns>
        private int StartCallbackService(Type service)
        {
            int port = Net.FindPort();

            var eventHandlerService = new ServiceHost(this);
            ServiceEndpoint se = eventHandlerService.AddServiceEndpoint(service, new WebHttpBinding(), Net.GetUrl(Ip, port, ""));
            se.Behaviors.Add(new WebHttpBehavior());
            try
            {
                eventHandlerService.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Basic Client: error launching callback service: " + ex);
                throw new ApplicationException(ex.ToString());
            }
            _callbackServices.Add(service, eventHandlerService);
            return port;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Register a given device with the activity client
        /// </summary>
        /// <param name="d">The device that needs to be registered with the activity client</param>
        public void Register(Device d)
        {
            d.BaseAddress = Net.GetIp(IPType.All);
            DeviceId = JsonConvert.DeserializeObject<String>(Rest.Post(ServiceAddress + Url.Devices, d));
            Console.WriteLine("BasicClient: Received device id: " + DeviceId);
        }
        
        /// <summary>
        /// Unregister main device from the activity client
        /// </summary>
        public void Unregister()
        {
            Rest.Delete(ServiceAddress + Url.Devices, DeviceId);
        }

        /// <summary>
        /// Subscribe the activity client to an activity manager event
        /// </summary>
        /// <param name="type">The type of event for which the client needs to subscribe</param>
        public void Subscribe(EventType type)
        {
            int port = StartCallbackService(EventTypeConverter.TypeFromEnum(type));
            var subscription = new
            {
                id = DeviceId, port, type
            };
            Rest.Post(ServiceAddress + Url.Subscribers, subscription);
        }

        /// <summary>
        /// Unsubscribe the activity client from an activity client event
        /// </summary>
        /// <param name="type">The type of event to which the client has to unsubscribe</param>
        public void UnSubscribe(EventType type)
        {
            var unSubscription = new
            {
                id = DeviceId, type
            };

            Type t = EventTypeConverter.TypeFromEnum(type);
            if (_callbackServices.ContainsKey(t))
            {
                _callbackServices[t].Close();
                _callbackServices.Remove(t);
                Rest.Delete(ServiceAddress + Url.Subscribers, unSubscription);
            }
        }

        /// <summary>
        /// Unsubscribe the client for all events
        /// </summary>
        public void UnSubscribeAll()
        {
            foreach (EventType ev in Enum.GetValues(typeof(EventType)))
                UnSubscribe(ev);
        }

        /// <summary>
        /// Sends an "add activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void AddActivity(Activity act)
        {
            Rest.Post(ServiceAddress + Url.Activities, act);
        }

/*
        /// <summary>
        /// Get the file byte array from the resource
        /// </summary>
        /// <param name="resource">The resource describing the file</param>
        /// <returns>A byte array representing the file</returns>
        private byte[] GetFile(Resource resource)
        {
            var fi = new FileInfo(LocalPath + resource.RelativePath);
            byte[] buffer = new byte[fi.Length];

            using (var fs = new FileStream(LocalPath + resource.RelativePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);
            return buffer;
        }
*/

        /// <summary>
        /// Sends a "Remove activity" request to the activity manager
        /// </summary>
        /// <param name="id">The id (of the activity) that needs to be included in the request</param>
        public void RemoveActivity(Guid id)
        {
            Rest.Delete(ServiceAddress + Url.Activities, id);
        }

        /// <summary>
        /// Sends an "Update activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void UpdateActivity(Activity act)
        {
            Rest.Put(ServiceAddress + Url.Activities, act);
        }

        /// <summary>
        /// Sends a "Get Activities" request to the activity manager
        /// </summary>
        /// <returns>A list of retrieved activities</returns>
        public List<Activity> GetActivities()
        {
            var res = Rest.Get(ServiceAddress + Url.Activities);
            return JsonConvert.DeserializeObject<List<Activity>>(res);
        }

        /// <summary>
        /// Sends a "Get Activity" request to the activity manager
        /// </summary>
        /// <param name="id">The id (of the activity) that needs to be included in the request</param>
        /// <returns></returns>
        public Activity GetActivity(string id)
        {
            return JsonConvert.DeserializeObject<Activity>(Rest.Get(ServiceAddress + Url.Activities + "/" + id));
        }

        /// <summary>
        /// Sends a "Send Message" request to the activity manager
        /// </summary>
        /// <param name="msg">The message that needs to be included in the request</param>
        public void SendMessage(string msg)
        {
            var message = new
            {
                id = DeviceId,
                message = msg
            };
            Rest.Post(ServiceAddress + Url.Messages, message);
        }

        /// <summary>
        /// Gets all users in the friendlist
        /// </summary>
        /// <returns>A list with all users in the friendlist</returns>
        public List<User> GetUsers()
        {
            return JsonConvert.DeserializeObject<List<User>>(Rest.Get(ServiceAddress + Url.Users)); 
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            JsonConvert.DeserializeObject<List<User>>(Rest.Post(ServiceAddress + Url.Users,email)); 
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            JsonConvert.DeserializeObject<List<User>>(Rest.Delete(ServiceAddress + Url.Users, friendId)); 
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            Rest.Put(ServiceAddress + Url.Users, new{friendId, approval});
        }

        #endregion

        #region Internal Event Handlers
        protected void OnConnectionEstablishedEvent(EventArgs e)
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished(this, e);
        }
        protected void OnInitializedEvent(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
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
        Files
    }
}
