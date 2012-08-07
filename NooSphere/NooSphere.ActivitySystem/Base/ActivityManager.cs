/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.IO;

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.PubSub;
using NooSphere.ActivitySystem.FileServer;

namespace NooSphere.ActivitySystem.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    public class ActivityManager : IActivityManager
    {
        #region Private Members
        private RestSubscriber _subscriber;
        private RestPublisher _publisher;
        private ActivityCloudConnector _activityCloudConnector;
        private FileStore _fileServer;

        private bool _connectionActive;
        private readonly bool _useLocalCloud;
        private readonly bool _useCloud;
        #endregion

        #region Public Members
        public User Owner { get; set; }
        #endregion

        #region Constructor
        public ActivityManager(User owner,string localPath,bool useLocalCloud=false, bool useCloud=true)
        {
            Owner = owner;
            _useLocalCloud = useLocalCloud;
            _useCloud = useCloud;

            IntializeEventSystem();
            InitializeFileService(localPath);
            InitializeActivityService(Owner);
        }
        #endregion

        #region Initializers
        private void InitializeActivityService(User owner)
        {
            if(_useCloud)
                ConnectToCloud(owner);
        }

        private void InitializeFileService(string localPath)
        {
            _fileServer = new FileStore(localPath);
            _fileServer.FileAdded += FileServerFileAdded;
            _fileServer.FileChanged += FileServerFileChanged;
            _fileServer.FileRemoved += FileServerFileRemoved;
            _fileServer.FileDownloadedFromCloud += FileServerFileDownloaded;
            Console.WriteLine("ActivityManager: FileStore initialized at {0}", _fileServer.BasePath);
        }

        private void IntializeEventSystem()
        {
            Registry.Initialize();
            _subscriber = new RestSubscriber();
            _publisher = new RestPublisher();

            Console.WriteLine("ActivityManager: Event System initialized");
        }
        #endregion

        #region Net

        /// <summary>
        /// Creates a new activitycloud connection
        /// </summary>
        /// <param name="owner">The current user</param>
        private void ConnectToCloud(User owner)
        {
            var serviceAddress = _useLocalCloud ? "http://10.1.1.190:56002" : "http://activitycloud-1.apphb.com";

            _activityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", owner);
            _activityCloudConnector.ConnectionSetup += ActivityCloudConnectorConnectionSetup;
            _activityCloudConnector.ActivityAdded += ActivityCloudConnectorActivityAdded;
            _activityCloudConnector.ActivityDeleted += ActivityCloudConnectorActivityDeleted;
            _activityCloudConnector.ActivityUpdated += ActivityCloudConnectorActivityUpdated;

            _activityCloudConnector.FileDeleteRequest += ActivityCloudConnectorFileDeleteRequest;
            _activityCloudConnector.FileDownloadRequest += ActivityCloudConnector_FileDownloadRequest;
            _activityCloudConnector.FileUploadRequest += ActivityCloudConnectorFileUploadRequest;

            _activityCloudConnector.FriendDeleted += ActivityCloudConnectorFriendDeleted;
            _activityCloudConnector.FriendAdded += ActivityCloudConnectorFriendAdded;
            _activityCloudConnector.FriendRequestReceived += ActivityCloudConnectorFriendRequestReceived;
            _activityCloudConnector.ParticipantAdded += ActivityCloudConnectorParticipantAdded;
            _activityCloudConnector.ParticipantRemoved += ActivityCloudConnectorParticipantRemoved;

            Console.WriteLine("ActivityManager: Cloud connnector connected to {0}", serviceAddress); 
        }

        /// <summary>
        /// Constructs a local activity cache for offline use
        /// </summary>
        private void ConstructActivityCache()
        {
            var t = new Thread(() =>
            {
                foreach (var act in _activityCloudConnector.GetActivities())
                {
                    ActivityStore.Activities.Add(act.Id, act);
                    foreach (var res in act.GetResources())
                        if (res != null)
                            _fileServer.AddFile(res, _activityCloudConnector.GetResource(res),FileSource.Cloud);
                    _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
                }
                Console.WriteLine("ActivityManager: Activity Store intialized"); 
            }) {IsBackground = true};
            t.Start();
        }
        #endregion

        #region Net Handlers
        private void FileServerFileDownloaded(object sender, FileEventArgs e)
        {
            _publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
        }
        private void FileServerFileRemoved(object sender, FileEventArgs e)
        {
            _publisher.Publish(EventType.FileEvents, FileEvent.FileDeleteRequest.ToString(), e.Resource);
            if (_connectionActive && _useCloud)
                _activityCloudConnector.DeleteFile(e.Resource);
        }
        private void FileServerFileChanged(object sender, FileEventArgs e)
        {
            _publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
            if (_connectionActive && _useCloud)
                _activityCloudConnector.AddResource(e.Resource, _fileServer.GetFile(e.Resource));
        }
        private void FileServerFileAdded(object sender, FileEventArgs e)
        {
            _publisher.Publish(EventType.FileEvents, FileEvent.FileDownloadRequest.ToString(), e.Resource);
            if (_connectionActive && _useCloud)
                _activityCloudConnector.AddResource(e.Resource, _fileServer.GetFile(e.Resource));
        }

        private void ActivityCloudConnector_FileDownloadRequest(object sender, FileEventArgs e)
        {
           Console.WriteLine("Cloud download request from file: " +e.Resource.RelativePath);
        }
        private void ActivityCloudConnectorFileDeleteRequest(object sender, FileEventArgs e)
        {
            _publisher.Publish(EventType.FileEvents, FileEvent.FileDeleteRequest.ToString(), e.Resource);
        }
        private void ActivityCloudConnectorConnectionSetup(object sender, EventArgs e)
        {
            _connectionActive = true;
            ConstructActivityCache();
        }
        private void ActivityCloudConnectorParticipantRemoved(object sender, ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Remove(e.Participant);
            var participantRemovedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            _publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantRemovedToActivity);
        }
        private void ActivityCloudConnectorParticipantAdded(object sender, ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Add(e.Participant);
            var participantAddedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            _publisher.Publish(EventType.UserEvent, UserEvents.ParticipantAdded.ToString(), participantAddedToActivity);
        }
        private void ActivityCloudConnectorFriendRequestReceived(object sender, FriendEventArgs e)
        {
            _publisher.Publish(EventType.UserEvent, UserEvents.FriendRequest.ToString(), e.User);
        }
        private void ActivityCloudConnectorFriendAdded(object sender, FriendEventArgs e)
        {
            _publisher.Publish(EventType.UserEvent, UserEvents.FriendAdded.ToString(), e.User);
        }
        private void ActivityCloudConnectorFriendDeleted(object sender, FriendDeletedEventArgs e)
        {
            _publisher.Publish(EventType.UserEvent, UserEvents.FriendRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnectorFileUploadRequest(object sender, FileEventArgs e)
        {
            byte[] buffer = _fileServer.GetFile(e.Resource);
            _activityCloudConnector.AddResource(e.Resource, buffer);
        }
        private void ActivityCloudConnectorActivityUpdated(object sender, ActivityEventArgs e)
        {
            ActivityStore.Activities[e.Activity.Id] = e.Activity;
            _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), e.Activity);
        }
        private void ActivityCloudConnectorActivityDeleted(object sender, ActivityRemovedEventArgs e)
        {
            ActivityStore.Activities.Remove(e.Id);
            _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnectorActivityAdded(object sender, ActivityEventArgs e)
        {
            if (!ActivityStore.Activities.ContainsKey(e.Activity.Id))
            {
                ActivityStore.Activities.Add(e.Activity.Id, e.Activity);
                _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), e.Activity);
            }
        }

        #endregion

        #region Helper

        /// <summary>
        /// Help function that allows the client to "ping" the service.
        /// </summary>
        /// <returns></returns>
        public bool Alive() 
        { 
            return _connectionActive; 
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
            return _useCloud ? _activityCloudConnector.GetActivity(id) : ActivityStore.Activities[id];
        }

        /// <summary>
        /// Gets an activity based on a given string id
        /// </summary>
        /// <param name="id">String representing the guid</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>The activity identified by the string</returns>
        public Activity GetActivity(string id)
        {
            if (id == null) 
                throw new ArgumentNullException("id");
            return _useCloud ? _activityCloudConnector.GetActivity(new Guid(id)) : ActivityStore.Activities[new Guid(id)];
        }

        /// <summary>
        /// Gets a list of all activities
        /// </summary>
        /// <returns>All activities for the current user</returns>
        public List<Activity> GetActivities()
        {
            return _useCloud ? _activityCloudConnector.GetActivities() : new List<Activity>(ActivityStore.Activities.Values);
        }

        /// <summary>
        /// Adds an activity to the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be added to the cloud</param>
        public void AddActivity(Activity act)
        {
            PublishActivity(act);
        }

        #region WIP

        private void PublishActivity(Activity act)
        {
            if (_useLocalCloud && _connectionActive)
                _activityCloudConnector.AddActivity(act);
            ActivityStore.Activities.Add(act.Id, act);
            _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityAdded.ToString(), act);
        }
        #endregion

        /// <summary>
        /// Removes an activity from the cloud
        /// </summary>
        /// <param name="id">The id of the activity that needs to be removed</param>
        public void RemoveActivity(string id)
        {
            if (_useLocalCloud && _connectionActive)
                _activityCloudConnector.DeleteActivity(new Guid(id));
            ActivityStore.Activities.Remove(new Guid(id));
            _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityRemoved.ToString(), id);
        }

        /// <summary>
        /// Updates an activity in the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be updated</param>
        public void UpdateActivity(Activity act)
        {
            if (_useLocalCloud && _connectionActive)
                _activityCloudConnector.UpdateActivity(act);
            ActivityStore.Activities[act.Id] = act;
            _publisher.Publish(EventType.ActivityEvents, ActivityEvent.ActivityChanged.ToString(), act);
        }
        #endregion

        #region Participant Management
        public void AddParticipant(Activity a, User u)
        {
            if (_useLocalCloud && _connectionActive)
                _activityCloudConnector.AddParticipant(a.Id, u.Id);
            ParticipantStore.Participants.Add(u.Id, u);
        }
        public void RemoveParticipant(Activity a, string id)
        {
            if (_useLocalCloud && _connectionActive)
                _activityCloudConnector.RemoveParticipant(a.Id, new Guid(id));
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
            return _useLocalCloud && _connectionActive ? _activityCloudConnector.GetUsers(Owner.Id) : null;
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            _activityCloudConnector.RequestFriendShip(Owner.Id, _activityCloudConnector.GetIdFromUserEmail(email));
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            _activityCloudConnector.RemoveFriend(Owner.Id, friendId);
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            _activityCloudConnector.RespondToFriendRequest(Owner.Id, friendId,approval);
        }

        #endregion

        #region Pub/Sub
        public Guid Register(Device device)
        {
            var cc = new ConnectedClient(device.Name, device.BaseAddress, device);
            if (Registry.ConnectedClients.ContainsKey(device.Id.ToString()))
                return new Guid("null");
            Registry.ConnectedClients.Add(device.Id.ToString(), cc);
            _publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceAdded.ToString(), device);
            return device.Id;
        }

        public void Subscribe(string id, EventType type,int callbackPort)
        {
            lock (Concurrency.SubscriberLock)
            {
                if (id != null)
                {
                    _subscriber.Subscribe(id, type, callbackPort);
                }
            }
        }
        public void UnSubscribe(string id, EventType type)
        {
            if(id !=null)
                _subscriber.UnSubscribe(id,type);
        }
        public void UnRegister(string id)
        {
            if(id != null)
                if(Registry.ConnectedClients.ContainsKey(id))
                {
                    _publisher.Publish(EventType.DeviceEvents, DeviceEvent.DeviceRemoved.ToString(), id);
                    Registry.ConnectedClients.Remove(id);
                }
        }
        #endregion

        #region Messenger
        public void SendMessage(string id, string message)
        {
            _publisher.Publish(EventType.ComEvents, ComEvent.MessageReceived.ToString(), message);
        }
        #endregion

        #region File Server
        public void AddFile(string activityId, string resourceId, Stream stream)
        {
            var resource = GetResourceFromId(activityId, resourceId);
            var buffer = new byte[resource.Size];
            var ms = new MemoryStream();
            int bytesRead, totalBytesRead = 0;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;

                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            _fileServer.AddFile(resource, buffer,FileSource.Local);
            ms.Close();
            Console.WriteLine("ActivityManager: Streamed file {0} into {1} bytes", resource.Name, totalBytesRead);
        }

        private Resource GetResourceFromId(string aId, string resId)
        {
            return ActivityStore.Activities[new Guid(aId)].GetResources().FirstOrDefault(
                res => (res.Id.ToString() == resId) && (res.ActivityId.ToString() == aId));
        }

        public void RemoveFile(Resource resource)
        {
            _fileServer.RemoveFile(resource);
        }

        public void UpdateFile(Resource resource, byte[] fileInBytes)
        {
            _fileServer.Updatefile(resource, fileInBytes);
        }

        public List<Resource> Sync()
        {
            return new List<Resource>();
        }
        #endregion
    }
}
