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
        private bool connectionActive = false;

        #endregion

        #region Public Members
        public User Owner { get; set; }
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
            Registry.Register(EventType.UserEvent);

            if(useActivityCloud)
                ConnectToCloud(useLocalCloud,owner);

            this.Owner = owner;
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
                serviceAddress = "http://10.1.1.190:56002";
            else
                serviceAddress = "http://activitycloud-1.apphb.com";

            ActivityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", @"C:\abc\", owner);
            ActivityCloudConnector.ConnectionSetup += new EventHandler(ActivityCloudConnector_ConnectionSetup);
            ActivityCloudConnector.ActivityAdded += new ActivitySystem.Events.ActivityAddedHandler(ActivityCloudConnector_ActivityAdded);
            ActivityCloudConnector.ActivityDeleted += new ActivitySystem.Events.ActivityRemovedHandler(ActivityCloudConnector_ActivityDeleted);
            ActivityCloudConnector.ActivityUpdated += new ActivitySystem.Events.ActivityChangedHandler(ActivityCloudConnector_ActivityUpdated);

            ActivityCloudConnector.FileDeleted += new Events.FileDeletedHandler(ActivityCloudConnector_FileDeleted);
            ActivityCloudConnector.FileDownloaded += new Events.FileDownloadedHandler(ActivityCloudConnector_FileDownloaded);
            ActivityCloudConnector.FileUploaded += new Events.FileUploadedHandler(ActivityCloudConnector_FileUploaded);
            ActivityCloudConnector.FriendDeleted += new Events.FriendDeletedHandler(ActivityCloudConnector_FriendDeleted);
            ActivityCloudConnector.FriendAdded += new Events.FriendAddedHandler(ActivityCloudConnector_FriendAdded);
            ActivityCloudConnector.FriendRequestReceived += new Events.FriendRequestReceivedHandler(ActivityCloudConnector_FriendRequestReceived);
            ActivityCloudConnector.ParticipantAdded += new Events.ParticipantAddedHandler(ActivityCloudConnector_ParticipantAdded);
            ActivityCloudConnector.ParticipantRemoved += new Events.ParticipantRemovedHandler(ActivityCloudConnector_ParticipantRemoved);

            Console.WriteLine("Local Activity Manager: Attempting to connect to: " + serviceAddress);
        }

        #region Net Handlers
        private void ActivityCloudConnector_ParticipantRemoved(object sender, Events.ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Remove(e.Participant);
            var participantRemovedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantRemovedToActivity);
        }
        private void ActivityCloudConnector_ParticipantAdded(object sender, Events.ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Add(e.Participant);
            var participantAddedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantAddedToActivity);
        }
        private void ActivityCloudConnector_FriendRequestReceived(object sender, Events.FriendEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendRequest.ToString(), e.User);
        }
        private void ActivityCloudConnector_FriendAdded(object sender, Events.FriendEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendAdded.ToString(), e.User);
        }
        private void ActivityCloudConnector_FriendDeleted(object sender, Events.FriendDeletedEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnector_FileUploaded(object sender, Events.FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileAdded.ToString(), e.Resource);
        }
        private void ActivityCloudConnector_FileDownloaded(object sender, Events.FileEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void ActivityCloudConnector_FileDeleted(object sender, Events.FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileRemoved.ToString(), e.Resource);
        }
        private void ActivityCloudConnector_ConnectionSetup(object sender, EventArgs e)
        {
            connectionActive = true;
        }
        private void ActivityCloudConnector_ActivityUpdated(object sender, ActivitySystem.Events.ActivityEventArgs e)
        {
            ActivityStore.Activities[e.Activity.Id] = e.Activity;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), e.Activity);
        }
        private void ActivityCloudConnector_ActivityDeleted(object sender, ActivitySystem.Events.ActivityRemovedEventArgs e)
        {
            ActivityStore.Activities.Remove(e.ID);
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), e.ID);
        }
        private void ActivityCloudConnector_ActivityAdded(object sender, ActivitySystem.Events.ActivityEventArgs e)
        {
            if (!ActivityStore.Activities.ContainsKey(e.Activity.Id))
            {
                ActivityStore.Activities.Add(e.Activity.Id, e.Activity);
                publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), e.Activity);
            }
        }
        #endregion
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
                return ActivityCloudConnector.GetActivity(id);
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
                return ActivityCloudConnector.GetActivity(new Guid(id));
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
            if (useActivityCloud && connectionActive)
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
            if (useActivityCloud && connectionActive)
                ActivityCloudConnector.DeleteActivity(new Guid(id));
            ActivityStore.Activities.Remove(new Guid(id));
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), id);
        }

        /// <summary>
        /// Updates an activity in the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be updated</param>
        public void UpdateActivity(Activity act)
        {
            if (useActivityCloud && connectionActive)
                ActivityCloudConnector.UpdateActivity(act);
            ActivityStore.Activities[act.Id] = act;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), act);
        }
        #endregion

        #region Participant Management
        public void AddParticipant(Activity a, User u)
        {
            if (useActivityCloud && connectionActive)
                ActivityCloudConnector.AddParticipant(a.Id, u.Id);
            ParticipantStore.Participants.Add(u.Id, u);
        }
        public void RemoveParticipant(Activity a, string id)
        {
            if (useActivityCloud && connectionActive)
                ActivityCloudConnector.RemoveParticipant(a.Id, new Guid(id));
            ParticipantStore.Participants.Remove(new Guid(id));
        }
        #endregion

        #region User Management

        /// <summary>
        /// Gets all users in the friendlist
        /// </summary>
        /// <returns>A list with all users in the friendlist</returns>
        public List<User> GetUsers()
        {
            return ActivityCloudConnector.GetUsers(Owner.Id);
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            ActivityCloudConnector.RequestFriendShip(Owner.Id, ActivityCloudConnector.GetIdFromUserEmail(email));
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            ActivityCloudConnector.RemoveFriend(Owner.Id, friendId);
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            ActivityCloudConnector.RespondToFriendRequest(Owner.Id, friendId,approval);
        }

        #endregion

        #region Pub/Sub
        public Guid Register(Device device)
        {
            ConnectedClient cc = new ConnectedClient(device.Name, device.BaseAddress, device);
            if (!Registry.ConnectedClients.ContainsKey(device.Id.ToString()))
            {
                Registry.ConnectedClients.Add(device.Id.ToString(), cc);
                publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceAdded.ToString(), device);
                return device.Id;
            }
            else
                return new Guid("null");
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
            if(id !=null)
                subscriber.UnSubscribe(id,type);
        }
        public void UnRegister(string id)
        {
            if(id != null)
                if(Registry.ConnectedClients.ContainsKey(id))
                {
                    publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceRemoved.ToString(), id);
                    Registry.ConnectedClients.Remove(id);
                }
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
