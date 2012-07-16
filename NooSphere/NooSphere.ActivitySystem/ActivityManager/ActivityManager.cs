/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Web;
using System.ServiceModel.Web;

using Newtonsoft.Json;

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.ActivitySystem.PubSub;

namespace NooSphere.ActivitySystem.ActivityManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    public class ActivityManager : IActivityManager
    {
        #region Private Members
        private RestSubscriber subscriber;
        private RestPublisher publisher;
        private ActivityCloudConnector ActivityCloudConnector;

        private bool useActivityCloud = true;
        private bool useLocalCloud = false;

        #endregion

        #region Constructor
        public ActivityManager(User owner)
        {
            subscriber = new RestSubscriber();
            publisher = new RestPublisher();
         
            Registry.Register(EventType.ActivityEvents);
            Registry.Register(EventType.ComEvents);
            Registry.Register(EventType.DeviceEvents);
            Registry.Register(EventType.FileEvents);

            if(useActivityCloud)
                ConnectToCloud(useLocalCloud,owner);
        }
        #endregion

        #region Net
        /// <summary>
        /// Creates a new activtycloud connection
        /// </summary>
        /// <param name="useLocalcloud">Bool indicating if the local test cloud should be use</param>
        /// <param name="owner">The current user</param>
        private void ConnectToCloud(bool useLocalcloud,User owner)
        {
            var serviceAddress="";
            if (useLocalCloud)
                serviceAddress = "http://localhost:56002";
            else
                serviceAddress = "http://activitycloud-1.apphb.com";

            ActivityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", @"C:\abc\", owner);
            ActivityCloudConnector.ActivityAdded += new EventHandler<DataEventArgs>(ActivityCloudConnector_ActivityAdded);
            ActivityCloudConnector.ActivityDeleted += new EventHandler<DataEventArgs>(ActivityCloudConnector_ActivityDeleted);
            ActivityCloudConnector.ActivityUpdated += new EventHandler<DataEventArgs>(ActivityCloudConnector_ActivityUpdated);

            Console.WriteLine("Local Activity Manager: Attempting to connect to: " + serviceAddress);
        }

        private void ActivityCloudConnector_ActivityUpdated(object sender, DataEventArgs e)
        {
            object obj = e.Data;
            //publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), e.Data);
        }

        void ActivityCloudConnector_ActivityDeleted(object sender, DataEventArgs e)
        {
            object obj = e.Data;
        }

        void ActivityCloudConnector_ActivityAdded(object sender, DataEventArgs e)
        {
            object obj = e.Data;
        }
        #endregion

        #region Helper

        /// <summary>
        /// Help function that allows the client to "ping" the service.
        /// </summary>
        /// <returns></returns>
        public bool Alive() { return true; }
        #endregion

        #region Activity Management

        /// <summary>
        /// Gets an activity based on a given guid
        /// </summary>
        /// <param name="id">Guid representidentifying the activity</param>
        /// <returns>The activity identified by the guid</returns>
        public Activity GetActivity(Guid id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivity(id.ToString());
            else
                return ActivityStore.Activities[id];
        }

        /// <summary>
        /// Gets an activity based on a given string id
        /// </summary>
        /// <param name="id">String representing the guid</param>
        /// <returns>The activity identified by the string</returns>
        public Activity GetActivity(string id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivity(id);
            else
                return ActivityStore.Activities[new Guid(id)];
        }

        /// <summary>
        /// Gets a list of all activities
        /// </summary>
        /// <returns>All activities for the current user</returns>
        public List<Activity> GetActivities()
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivities();
            else
                return ActivityStore.Activities.Values.ToList();
        }
       
        /// <summary>
        /// Adds an activity to the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be added to the cloud</param>
        public void AddActivity(Activity act)
        {
            if (useActivityCloud)
                ActivityCloudConnector.AddActivity(act);
            ActivityStore.Activities.Add(act.Id, act);
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
        }

        /// <summary>
        /// Removes an activity from the cloud
        /// </summary>
        /// <param name="id">The id of the activity that needs to be removed</param>
        public void RemoveActivity(string id)
        {
            if (useActivityCloud)
                ActivityCloudConnector.DeleteActivity(id);
            ActivityStore.Activities.Remove(new Guid(id));
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), id);
        }

        /// <summary>
        /// Updates an activity in the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be updated</param>
        public void UpdateActivity(Activity act)
        {
            if (useActivityCloud)
                ActivityCloudConnector.UpdateActivity(act);
            ActivityStore.Activities[act.Id] = act;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), act);
        }
        #endregion

        #region Participant Management
        public User GetParticipant(string id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetUser(id);
            else
                return ParticipantStore.Participants[new Guid(id)];
        }
        public List<User> GetParticipants()
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetUsers();
            else
                return ParticipantStore.Participants.Values.ToList();
        }
        public void AddParticipant(Activity a, User p)
        {
            if (useActivityCloud)
                ActivityCloudConnector.AddUser(p);
            ParticipantStore.Participants.Add(p.Id, p);
        }
        public void RemoveParticipant(Activity a, string id)
        {
            if (useActivityCloud)
                ActivityCloudConnector.DeleteUser(id);
            ParticipantStore.Participants.Remove(new Guid(id));
        }
        public void UpdateParticipant(User p)
        {
            if (useActivityCloud)
                ActivityCloudConnector.UpdateUser(p);
            ParticipantStore.Participants[p.Id] = p;
        }
        #endregion

        #region Pub/Sub 
        public Guid Register(Device device)
        {
            ConnectedClient cc = new ConnectedClient(device.Name, device.BaseAddress, device);
            Registry.ConnectedClients.Add(device.Id.ToString(), cc);
            publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceAdded.ToString(), device);
            return device.Id;
        }
        public void Subscribe(string id, EventType type,int callbackPort)
        {
            lock (Concurrency._SubscriberLock)
            {
                if (id != null)
                {
                    subscriber.Subscribe(id, type, callbackPort);
                }
            }
        }
        public void UnSubscribe(string id, EventType type)
        {
           subscriber.UnSubscribe(id,type);
        }
        public void UnRegister(string id)
        {
            publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceRemoved.ToString(), id);
            Registry.ConnectedClients.Remove(id);
        }
        #endregion

        #region Messenger
        public void SendMessage(string id, string message)
        {
            publisher.Publish(EventType.ComEvents, ComEvent.MessageReceived.ToString(), message);
        }
        #endregion
    }

}
