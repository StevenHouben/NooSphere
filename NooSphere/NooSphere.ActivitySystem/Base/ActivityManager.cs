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
using NooSphere.ActivitySystem.PubSub;
using NooSphere.ActivitySystem.FileServer;

namespace NooSphere.ActivitySystem
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    public class ActivityManager : IActivityManager,IFileHandler
    {
        #region Private Members
        private RestSubscriber subscriber;
        private RestPublisher publisher;
        private ActivityCloudConnector activityCloudConnector;

        private bool useActivityCloud = true;
        private bool useLocalCloud = false;
        private bool connectionActive = false;

        private FileStore fileServer;

        #endregion

        #region Public Members
        public User Owner { get; set; }
        public string LocalPath { get; private set; }
        #endregion

        #region Constructor
        public ActivityManager(User owner,string localPath)
        {
            this.Owner = owner;

            IntializeEventSystem();
            InitializeFileService(localPath);
            InitializeActivityService(Owner);
        }
        private void InitializeActivityService(User owner)
        {
            if (useActivityCloud)
                ConnectToCloud(useLocalCloud, owner);
        }
        private void InitializeFileService(string localPath)
        {
            this.LocalPath = localPath;

            fileServer = new FileStore(LocalPath); ;
            fileServer.FileAdded += new FileAddedHandler(fileServer_FileAdded);
            fileServer.FileChanged += new FileChangedHandler(fileServer_FileChanged);
            fileServer.FileRemoved += new FileRemovedHandler(fileServer_FileRemoved);
            fileServer.FileDownloadedFromCloud += new FileDownloadRequestHandler(fileServer_FileDownloaded);

            Console.WriteLine("ActivityManager: FileStore initialized at {0}", this.LocalPath); 
        }
        private void IntializeEventSystem()
        {
            Registry.Initialize();
            subscriber = new RestSubscriber();
            publisher = new RestPublisher();

            Console.WriteLine("ActivityManager: Event System initialized"); 
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

            activityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", owner);
            activityCloudConnector.ConnectionSetup += new EventHandler(ActivityCloudConnector_ConnectionSetup);
            activityCloudConnector.ActivityAdded += new ActivityAddedHandler(ActivityCloudConnector_ActivityAdded);
            activityCloudConnector.ActivityDeleted += new ActivityRemovedHandler(ActivityCloudConnector_ActivityDeleted);
            activityCloudConnector.ActivityUpdated += new ActivityChangedHandler(ActivityCloudConnector_ActivityUpdated);

            activityCloudConnector.FileDeleteRequest += new FileDeleteRequestHandler(ActivityCloudConnector_FileDeleteRequest);
            activityCloudConnector.FileDownloadRequest += new FileDownloadRequestHandler(ActivityCloudConnector_FileDownloadRequest);
            activityCloudConnector.FileUploadRequest += new FileUploadRequestHandler(ActivityCloudConnector_FileUploadRequest);

            activityCloudConnector.FriendDeleted += new FriendDeletedHandler(ActivityCloudConnector_FriendDeleted);
            activityCloudConnector.FriendAdded += new FriendAddedHandler(ActivityCloudConnector_FriendAdded);
            activityCloudConnector.FriendRequestReceived += new FriendRequestReceivedHandler(ActivityCloudConnector_FriendRequestReceived);
            activityCloudConnector.ParticipantAdded += new ParticipantAddedHandler(ActivityCloudConnector_ParticipantAdded);
            activityCloudConnector.ParticipantRemoved += new ParticipantRemovedHandler(ActivityCloudConnector_ParticipantRemoved);

            Console.WriteLine("ActivityManager: Cloud connnector connected to {0}", serviceAddress); 
        }

        /// <summary>
        /// Constructs a local activity cache for offline use
        /// </summary>
        private void ConstructActivityCache()
        {
            Thread t = new Thread(() =>
            {
                foreach (Activity act in activityCloudConnector.GetActivities())
                {
                    ActivityStore.Activities.Add(act.Id, act);
                    foreach (Resource res in act.GetResources())
                        fileServer.AddFile(res, activityCloudConnector.GetResource(res),FileSource.Cloud);
                    publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
                }
                Console.WriteLine("ActivityManager: Activity Store intialized"); 
            });
            t.IsBackground = true;
            t.Start();
        }

        #region Net Handlers
        private void fileServer_FileDownloaded(object sender, FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
        }
        private void fileServer_FileRemoved(object sender, FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileDeleteRequest.ToString(), e.Resource);
            if (useActivityCloud && connectionActive)
                activityCloudConnector.DeleteFile(e.Resource);
        }
        private void fileServer_FileChanged(object sender, FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
            if (useActivityCloud && connectionActive)
                activityCloudConnector.AddResource(e.Resource, fileServer.GetFile(e.Resource));
        }
        private void fileServer_FileAdded(object sender, FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
            if (useActivityCloud && connectionActive)
                activityCloudConnector.AddResource(e.Resource, fileServer.GetFile(e.Resource));
        }

        private void ActivityCloudConnector_FileDownloadRequest(object sender, FileEventArgs e)
        {
           Console.WriteLine("Cloud download request from file: " +e.Resource.RelativePath);
        }
        private void ActivityCloudConnector_FileDeleteRequest(object sender, FileEventArgs e)
        {
            publisher.Publish(EventType.FileEvents, FileEvent.FileDeleteRequest.ToString(), e.Resource);
        }
        private void ActivityCloudConnector_ConnectionSetup(object sender, EventArgs e)
        {
            connectionActive = true;

            ConstructActivityCache();
        }
        private void ActivityCloudConnector_ParticipantRemoved(object sender, ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Remove(e.Participant);
            var participantRemovedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantRemovedToActivity);
        }
        private void ActivityCloudConnector_ParticipantAdded(object sender, ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Add(e.Participant);
            var participantAddedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantAddedToActivity);
        }
        private void ActivityCloudConnector_FriendRequestReceived(object sender, FriendEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendRequest.ToString(), e.User);
        }
        private void ActivityCloudConnector_FriendAdded(object sender, FriendEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendAdded.ToString(), e.User);
        }
        private void ActivityCloudConnector_FriendDeleted(object sender, FriendDeletedEventArgs e)
        {
            publisher.Publish(EventType.UserEvent, UserEvents.FriendRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnector_FileUploadRequest(object sender, FileEventArgs e)
        {
            byte[] buffer = fileServer.GetFile(e.Resource);
            activityCloudConnector.AddResource(e.Resource, buffer);
        }
        private void ActivityCloudConnector_ActivityUpdated(object sender, ActivitySystem.ActivityEventArgs e)
        {
            ActivityStore.Activities[e.Activity.Id] = e.Activity;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), e.Activity);
        }
        private void ActivityCloudConnector_ActivityDeleted(object sender, ActivitySystem.ActivityRemovedEventArgs e)
        {
            ActivityStore.Activities.Remove(e.ID);
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), e.ID);
        }
        private void ActivityCloudConnector_ActivityAdded(object sender, ActivitySystem.ActivityEventArgs e)
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
        public bool Alive() 
        { 
            return connectionActive; 
        }
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
                return activityCloudConnector.GetActivity(id);
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
                return activityCloudConnector.GetActivity(new Guid(id));
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
                return activityCloudConnector.GetActivities();
            else
                return ActivityStore.Activities.Values.ToList();
        }
       
        /// <summary>
        /// Adds an activity to the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be added to the cloud</param>
        public void AddActivity(Activity act)
        {
            //if (act.GetResources().Count > 0)
            //    ProcessActivity(act);
            PublishActivity(act);
        }

        #region WIP
        private void ProcessActivity(Activity act)
        {
            activityBuffer.Add(act.Id, act);
        }
        private void PublishActivity(Activity act)
        {
            if (useActivityCloud && connectionActive)
                activityCloudConnector.AddActivity(act);
            ActivityStore.Activities.Add(act.Id, act);
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
        }
        private Dictionary<Guid, Activity> activityBuffer = new Dictionary<Guid, Activity>();
        #endregion

        /// <summary>
        /// Removes an activity from the cloud
        /// </summary>
        /// <param name="id">The id of the activity that needs to be removed</param>
        public void RemoveActivity(string id)
        {
            if (useActivityCloud && connectionActive)
                activityCloudConnector.DeleteActivity(new Guid(id));
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
                activityCloudConnector.UpdateActivity(act);
            ActivityStore.Activities[act.Id] = act;
            publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), act);
        }
        #endregion

        #region Participant Management
        public void AddParticipant(Activity a, User u)
        {
            if (useActivityCloud && connectionActive)
                activityCloudConnector.AddParticipant(a.Id, u.Id);
            ParticipantStore.Participants.Add(u.Id, u);
        }
        public void RemoveParticipant(Activity a, string id)
        {
            if (useActivityCloud && connectionActive)
                activityCloudConnector.RemoveParticipant(a.Id, new Guid(id));
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
            if(useActivityCloud && connectionActive)
                return activityCloudConnector.GetUsers(Owner.Id);
            else
                return null;
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            activityCloudConnector.RequestFriendShip(Owner.Id, activityCloudConnector.GetIdFromUserEmail(email));
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            activityCloudConnector.RemoveFriend(Owner.Id, friendId);
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            activityCloudConnector.RespondToFriendRequest(Owner.Id, friendId,approval);
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

        #region File Server
        public void AddFile(string activityId, string resourceId, Stream stream)
        {
            Resource resource = GetResourceFromId(activityId, resourceId);
            byte[] buffer = new byte[resource.Size];
            MemoryStream ms = new MemoryStream();
            int bytesRead, totalBytesRead = 0;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;

                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            fileServer.AddFile(resource, buffer,FileSource.Local);
            ms.Close();
            Console.WriteLine("ActivityManager: Streamed file {0} into {1} bytes", resource.Name, totalBytesRead);
        }

        private Resource GetResourceFromId(string aId, string resId)
        {
            foreach (Resource res in ActivityStore.Activities[new Guid(aId)].GetResources())
                if ((res.Id.ToString() == resId) && (res.ActivityId.ToString() == aId))
                        return res;
            return null;
        }

        public void RemoveFile(Resource resource)
        {
            fileServer.RemoveFile(resource);
        }

        public void UpdateFile(Resource resource, byte[] fileInBytes)
        {
            fileServer.Updatefile(resource, fileInBytes);
        }

        public List<Resource> Sync()
        {
            return new List<Resource>();
        }
        #endregion
    }
}
