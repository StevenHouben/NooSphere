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

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;

using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.ActivitySystem.ActivityService.PubSub;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.ActivityService.ActivityManagement
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
        private void ConnectToCloud(bool useLocalcloud,User owner)
        {
            if (useLocalCloud)
            {
                var serviceAddress = "http://localhost:56002";
                ActivityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", @"C:\abc\",owner);
                Console.WriteLine("Local Activity Manager: Attempting to connect to local ActivityCloud.");
            }
            else
            {
                var serviceAddress = "http://activitycloud-1.apphb.com";
                ActivityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", @"C:\abc\",owner);
                Console.WriteLine("Local Activity Manager: Attempting to connect to appharbor ActivityCloud.");
            }
        }
        #endregion

        #region Helper
        public bool Alive() { return true; }
        #endregion

        #region Activity Management
        public object GetActivity(Guid id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivity(id.ToString());
            else
                return ActivityStore.Activities[id];
        }
        public object GetActivity(string id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivity(id);
            else
                return ActivityStore.Activities[new Guid(id)];
        }
        public object GetActivities()
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetActivities();
            else
                return ActivityStore.Activities.Values.ToList();
        }
        public void AddActivity(Activity act)
        {
            if (useActivityCloud)
                ActivityCloudConnector.AddActivity(act);
            ActivityStore.Activities.Add(act.Id, act);
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
        }
        public void RemoveActivity(string id)
        {
            if (useActivityCloud)
                ActivityCloudConnector.DeleteActivity(id);
            ActivityStore.Activities.Remove(new Guid(id));
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), id);
        }
        public void UpdateActivity(Activity act)
        {
            if (useActivityCloud)
                ActivityCloudConnector.UpdateActivity(act);
            ActivityStore.Activities[act.Id] = act;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), act);
        }
        #endregion

        #region Participant Management
        public object GetParticipant(Guid id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetUser(id.ToString());
            else
                return ParticipantStore.Participants[id];
        }
        public object GetParticipant(string id)
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetUser(id);
            else
                return ParticipantStore.Participants[new Guid(id)];
        }
        public object GetParticipants()
        {
            if (useActivityCloud)
                return ActivityCloudConnector.GetUsers();
            else
                return ParticipantStore.Participants;
        }
        public void AddParticipant(User p)
        {
            if (useActivityCloud)
                ActivityCloudConnector.AddUser(p);
            ParticipantStore.Participants.Add(p.Id, p);
        }
        public void RemoveParticipant(string id)
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
        public object Register(Device device)
        {
            ConnectedClient cc = new ConnectedClient(device.Name, device.BaseAddress, device);
            Registry.ConnectedClients.Add(device.Id.ToString(), cc);
            publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceAdded.ToString(), device);
            return device.Id.ToString();
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
