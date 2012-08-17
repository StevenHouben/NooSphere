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
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.IO;

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.PubSub;
using NooSphere.ActivitySystem.FileServer;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class ActivityManager : IActivityManager
    {
        #region Private Members
        private RestSubscriber _subscriber;
        private RestPublisher _publisher;
        private ActivityCloudConnector _activityCloudConnector;
        private FileService _fileServer;

        private Syncer localSyncer = new Syncer(SyncType.Local);
        private Syncer cloudSyncer = new Syncer(SyncType.Cloud);

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
        /// <summary>
        /// Initializes the Activity Service
        /// </summary>
        /// <param name="owner">User that owns this activity manager</param>
        private void InitializeActivityService(User owner)
        {
            if(_useCloud)
                ConnectToCloud(owner);
        }

        /// <summary>
        /// Initializes the File Service
        /// </summary>
        /// <param name="localPath">Path where the file service stores files</param>
        private void InitializeFileService(string localPath)
        {
            _fileServer = new FileService(localPath);
            _fileServer.FileAdded += FileServerFileAdded;
            _fileServer.FileChanged += FileServerFileChanged;
            _fileServer.FileRemoved += FileServerFileRemoved;
            _fileServer.FileDownloadedFromCloud += FileServerFileDownloaded;
            Log.Out("ActivityManager", string.Format("FileStore initialized at {0}", _fileServer.BasePath), LogCode.Log);
        }

        /// <summary>
        /// Initializes the Event Services (pub/sub)
        /// </summary>
        private void IntializeEventSystem()
        {
            _subscriber = new RestSubscriber();
            _publisher = new RestPublisher();

            Log.Out("ActivityManager", string.Format("Event System initialized"), LogCode.Log);
        }
        #endregion

        #region Net

        /// <summary>
        /// Creates a new activitycloud connection
        /// </summary>
        /// <param name="owner">The current user</param>
        private void ConnectToCloud(User owner)
        {
            var serviceAddress = _useLocalCloud ? "http://localhost:56002" : "http://activitycloud-1.apphb.com";

            _activityCloudConnector = new ActivityCloudConnector(serviceAddress + "/Api/", owner);
            _activityCloudConnector.ConnectionSetup += ActivityCloudConnectorConnectionSetup;
            _activityCloudConnector.ActivityAdded += ActivityCloudConnectorActivityAdded;
            _activityCloudConnector.ActivityDeleted += ActivityCloudConnectorActivityDeleted;
            _activityCloudConnector.ActivityUpdated += ActivityCloudConnectorActivityUpdated;

            _activityCloudConnector.FileDeleteRequest += ActivityCloudConnectorFileDeleteRequest;
            _activityCloudConnector.FileDownloadRequest += ActivityCloudConnectorFileDownloadRequest;
            _activityCloudConnector.FileUploadRequest += ActivityCloudConnectorFileUploadRequest;

            _activityCloudConnector.FriendDeleted += ActivityCloudConnectorFriendDeleted;
            _activityCloudConnector.FriendAdded += ActivityCloudConnectorFriendAdded;
            _activityCloudConnector.FriendRequestReceived += ActivityCloudConnectorFriendRequestReceived;
            _activityCloudConnector.ParticipantAdded += ActivityCloudConnectorParticipantAdded;
            _activityCloudConnector.ParticipantRemoved += ActivityCloudConnectorParticipantRemoved;

            Log.Out("ActivityManager", string.Format("Cloud connector connected to {0}", serviceAddress), LogCode.Net);
        }

        /// <summary>
        /// Constructs a local activity cache for offline use
        /// </summary>
        private void ConstructActivityCache()
        {
            //Log.Out("ActivityManager", string.Format("Activity Store intialized"), LogCode.Log);
            //var t = new Thread(() =>
            //{
            //    foreach (var act in _activityCloudConnector.GetActivities())
            //    {
            //        ActivityStore.Activities.Add(act.Id, act);
            //        _publisher.Publish(ActivityEvent.ActivityAdded.ToString(), act);
            //    }
            //}) { IsBackground = true };
            //t.Start();
        }

        /// <summary>
        /// Publish activity to cloud
        /// </summary>
        /// <param name="act">Activity that needs to be published</param>
        private void PublishActivity(Activity act,ActivityEvent aEvent)
        {
            Console.WriteLine("ActivityManager: Publishing activity {0} to cloud", act.Name);
            if (_useCloud && _connectionActive)
            {
                switch (aEvent)
                {
                    case ActivityEvent.ActivityAdded:
                        _activityCloudConnector.AddActivity(act);
                        break;
                    case ActivityEvent.ActivityChanged:
                        _activityCloudConnector.UpdateActivity(act);
                        break;
                }
            }

        }
        private void AddActivityFromCloud(Activity act)
        {
            //If the id is in the store -> activity was uploaded by this system
            if (ActivityStore.Activities.ContainsKey(act.Id)) return;
            
            //Initialize the activity
            _fileServer.IntializePath(act);

            //Add activity to the store 
            ActivityStore.Activities.Add(act.Id, act);

            //Publish activity added
            _publisher.Publish(ActivityEvent.ActivityAdded.ToString(), act);
        }

        private void KeepActivityFromCloud(Activity act, ActivityEvent aEvent)
        {
            Log.Out("ActivityManager", string.Format("Keeping activity {0} in buffer", act), LogCode.Log);
            cloudSyncer.Buffer.Add(act.Id, act);
            cloudSyncer.Counters.Add(act.Id, new Point(0, act.GetResources().Count));
            cloudSyncer.EventType.Add(act.Id, aEvent);
            foreach (var resource in act.GetResources())
                _activityCloudConnector.GetResource(resource);
        }

        /// <summary>
        /// Buffer activity locally until resources are uploaded
        /// </summary>
        /// <param name="act">Activity that needs to be buffered</param>
        /// <param name="deviceId">The id of the device </param>
        /// <param name="aEvent"> The type of event</param>
        private void KeepActivity(Activity act,string deviceId,ActivityEvent aEvent)
        {
            Log.Out("ActivityManager", string.Format("Keeping activity {0} in buffer", act), LogCode.Log);
 
            //Create a buffer id
            var bufferId = Guid.NewGuid();

            //Buffer the activity into a local buffer
            localSyncer.Buffer.Add(bufferId, act);
            localSyncer.Counters.Add(bufferId, new Point(0, act.GetResources().Count));
            localSyncer.EventType.Add(bufferId, aEvent);
            localSyncer.LookUpTable.Add(bufferId,act.Id);

            //Send File upload request to the client that has added the activity
            foreach (var resource in act.GetResources())
                _publisher.PublishToSubscriber(FileEvent.FileUploadRequest.ToString(), resource, Registry.ConnectedClients[deviceId]);
        }

        /// <summary>
        /// Track uploaded resources
        /// </summary>
        /// <param name="res">Resource that needs to be checked against activity buffer</param>
        /// <param name="syncer"> </param>
        private void ResourceTracker(Resource res,Syncer syncer)
        {
            var t = new Thread(() =>
            {
                if (syncer.Buffer.Count == 0)
                    return;
                foreach (var activity in syncer.Buffer.Values)
                {
                    foreach (var resource in activity.GetResources())
                    {
                        if (res.Id == resource.Id)
                        {
                            var counter = syncer.Counters[activity.Id];
                            counter.X++;
                            if (counter.X == counter.Y)
                            {
                                PublishActivity(activity, syncer.EventType[activity.Id]);
                                syncer.Counters.Remove(activity.Id);
                                syncer.Buffer.Remove(activity.Id);
                                syncer.EventType.Remove(activity.Id);

                                return;
                            }
                            syncer.Counters[activity.Id] = counter;
                        }
                    }
                }
            }) { IsBackground = true };
            t.Start();
        }
        #endregion

        #region Net Handlers
        private void FileServerFileDownloaded(object sender, FileEventArgs e)
        {
            _publisher.Publish( FileEvent.FileDownloadRequest.ToString(), e.Resource);
        }
        private void FileServerFileRemoved(object sender, FileEventArgs e)
        {
            _publisher.Publish(FileEvent.FileDeleteRequest.ToString(), e.Resource);
            if (_connectionActive && _useCloud)
                _activityCloudConnector.DeleteFile(e.Resource);
        }
        private void FileServerFileChanged(object sender, FileEventArgs e)
        {
            _publisher.Publish( FileEvent.FileDownloadRequest.ToString(), e.Resource);
            if (_connectionActive && _useCloud)
                _activityCloudConnector.AddResource(e.Resource, _fileServer.BasePath + e.Resource.RelativePath);
        }
        private void FileServerFileAdded(object sender, FileEventArgs e)
        {
            _publisher.Publish( FileEvent.FileDownloadRequest.ToString(), e.Resource);
            ResourceTracker(e.Resource,localSyncer);
        }
        private void ActivityCloudConnectorFileDownloadRequest(object sender, FileEventArgs e)
        {
            Log.Out("ActivityManager", string.Format("Cloud download request from file: " + e.Resource.RelativePath), LogCode.Log);
             _fileServer.AddFile(e.Resource, _activityCloudConnector.GetResource(e.Resource),
                                FileSource.ActivityCloud);
        }
        private void ActivityCloudConnectorFileDeleteRequest(object sender, FileEventArgs e)
        {
            _publisher.Publish(FileEvent.FileDeleteRequest.ToString(), e.Resource);
        }
        private void ActivityCloudConnectorConnectionSetup(object sender, EventArgs e)
        {
            Log.Out("ActivityManager", string.Format("ActivityManager: Connection Setup"), LogCode.Log);
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
            _publisher.Publish( UserEvents.FriendRemoved.ToString(), participantRemovedToActivity);
        }
        private void ActivityCloudConnectorParticipantAdded(object sender, ParticipantEventArgs e)
        {
            ActivityStore.Activities[e.ActivityId].Participants.Add(e.Participant);
            var participantAddedToActivity = new
            {
                u = e.Participant,
                activityId = e.ActivityId
            };
            _publisher.Publish( UserEvents.ParticipantAdded.ToString(), participantAddedToActivity);
        }
        private void ActivityCloudConnectorFriendRequestReceived(object sender, FriendEventArgs e)
        {
            _publisher.Publish( UserEvents.FriendRequest.ToString(), e.User);
        }
        private void ActivityCloudConnectorFriendAdded(object sender, FriendEventArgs e)
        {
            _publisher.Publish( UserEvents.FriendAdded.ToString(), e.User);
        }
        private void ActivityCloudConnectorFriendDeleted(object sender, FriendDeletedEventArgs e)
        {
            _publisher.Publish( UserEvents.FriendRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnectorFileUploadRequest(object sender, FileEventArgs e)
        {
            _activityCloudConnector.AddResource(e.Resource, _fileServer.BasePath +e.Resource.RelativePath);
            Log.Out("ActivityManager", string.Format("Cloud upload request from file: " + e.Resource.RelativePath), LogCode.Log);
        }
        private void ActivityCloudConnectorActivityUpdated(object sender, ActivityEventArgs e)
        {
            ActivityStore.Activities[e.Activity.Id] = e.Activity;
            _publisher.Publish( ActivityEvent.ActivityChanged.ToString(), e.Activity);
        }
        private void ActivityCloudConnectorActivityDeleted(object sender, ActivityRemovedEventArgs e)
        {
            ActivityStore.Activities.Remove(e.Id);
            _publisher.Publish( ActivityEvent.ActivityRemoved.ToString(), e.Id);
        }
        private void ActivityCloudConnectorActivityAdded(object sender, ActivityEventArgs e)
        {
            AddActivityFromCloud(e.Activity);
        }
        #endregion

        #region Helper

        /// <summary>
        /// Help function that allows the client to "ping" the service.
        /// </summary>
        /// <returns></returns>
        public bool Alive()
        {
            return !_useCloud || _connectionActive;
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
        /// <param name="deviceId"> </param>
        public void AddActivity(Activity act,string deviceId)
        {
            //Set the path of newly added activity
            _fileServer.IntializePath(act);

            //Check for resources
            if (act.GetResources().Count > 0)
            {
                //Buffer the activity (for the cloud sync) untill all files are uploaded
                KeepActivity(act,deviceId,ActivityEvent.ActivityAdded);
                Console.WriteLine("ActivityManager: Received activity with {0} resources",act.GetResources().Count);
            }
            else
            {
                //Update cloud
                Console.WriteLine("ActivityManager: Received activity with 0 resources");
                if (_connectionActive)
                    _activityCloudConnector.AddActivity(act);
            }

            //Publish Activity Added to the local systems
            ActivityStore.Activities.Add(act.Id, act);
            _publisher.Publish(ActivityEvent.ActivityAdded.ToString(), act);
            Console.WriteLine("ActivityManager: Published {0}: {1}", EventType.ActivityEvents, ActivityEvent.ActivityAdded);
        }

        /// <summary>
        /// Removes an activity from the cloud
        /// </summary>
        /// <param name="id">The id of the activity that needs to be removed</param>
        /// <param name="deviceId"> </param>
        public void RemoveActivity(string id, string deviceId)
        {
            if (_useCloud && _connectionActive)
                _activityCloudConnector.DeleteActivity(new Guid(id));
            ActivityStore.Activities.Remove(new Guid(id));
            _publisher.Publish( ActivityEvent.ActivityRemoved.ToString(), id);
            Console.WriteLine("ActivityManager: Published {0}: {1}", EventType.ActivityEvents, ActivityEvent.ActivityRemoved);
        }

        /// <summary>
        /// Updates an activity in the cloud
        /// </summary>
        /// <param name="act">The activity that needs to be updated</param>
        /// <param name="deviceId"> </param>
        public void UpdateActivity(Activity act, string deviceId)
        {
            //Check if the activity has resources
            if (act.GetResources().Count > 0)
            {
                //Buffer the activities until all resources are uploaded
                KeepActivity(act, deviceId, ActivityEvent.ActivityChanged);
                Console.WriteLine("ActivityManager: Received activity with {0} resources", act.GetResources().Count);
            }
            else
            {
                //No resource, so update to the cloud
                Console.WriteLine("ActivityManager: Received activity with 0 resources");
                if (_connectionActive)
                    _activityCloudConnector.UpdateActivity(act);
            }

            //Publish the activityChangedEvent to local system
            ActivityStore.Activities[act.Id] = act;
            _publisher.Publish(ActivityEvent.ActivityChanged.ToString(), act);
            Console.WriteLine("ActivityManager: Published {0}: {1}", EventType.ActivityEvents, ActivityEvent.ActivityChanged);
        }
        #endregion

        #region File Server
        /// <summary>
        /// Upload a file to the file server
        /// </summary>
        /// <param name="activityId">Id of the activity</param>
        /// <param name="resourceId">Id of the resource</param>
        /// <param name="stream">Stream</param>
        public void AddFile(string activityId, string resourceId, Stream stream)
        {
            //Get the resource
            var resource = GetResourceFromId(activityId, resourceId);
        
            //Add the file to the fileserver
            _fileServer.AddFile(resource,stream,FileSource.ActivityManager);
        }

        /// <summary>
        /// Gets a resource from id's
        /// </summary>
        /// <param name="aId">Id of the activity</param>
        /// <param name="resId">Id of the resource</param>
        /// <returns>Resource</returns>
        private Resource GetResourceFromId(string aId, string resId)
        {
            return ActivityStore.Activities[new Guid(aId)].GetResources().FirstOrDefault(
                res => (res.Id.ToString() == resId) && (res.ActivityId.ToString() == aId));
        }

        /// <summary>
        /// Removes a file from the File server
        /// </summary>
        /// <param name="resource">Resource that represents the file</param>
        public void RemoveFile(Resource resource)
        {
            _fileServer.RemoveFile(resource);
            Console.WriteLine("ActivityManager: Deleted file {0}", resource.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public Stream GetFile(string activityId, string resourceId)
        {
            return _fileServer.GetStreamFromFile(GetResourceFromId(activityId,resourceId));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="resourceId"></param>
        /// <param name="stream"></param>
        public void UpdateFile(string activityId, string resourceId, Stream stream)
        {
            Console.WriteLine("ActivityManager: Not implemented");
        }
        #endregion

        #region Participant Management

        /// <summary>
        /// Adds a participant to an activity
        /// </summary>
        /// <param name="a">Activity</param>
        /// <param name="u">Participant</param>
        /// <param name="deviceId"> </param>
        public void AddParticipant(Activity a, User u, string deviceId)
        {
            if (_useCloud && _connectionActive)
                _activityCloudConnector.AddParticipant(a.Id, u.Id);
            ParticipantStore.Participants.Add(u.Id, u);
            Console.WriteLine("ActivityManager: Added participant {0} to activity {1}", u.Name,a.Name);
        }

        /// <summary>
        /// Removes an participant from an activity
        /// </summary>
        /// <param name="a">Activity</param>
        /// <param name="id">Id that represents the participant</param>
        /// <param name="deviceId"> </param>
        public void RemoveParticipant(Activity a, string id, string deviceId)
        {
            if (_useCloud && _connectionActive)
                _activityCloudConnector.RemoveParticipant(a.Id, new Guid(id));
            Console.WriteLine("ActivityManager: Removed participant {0} to activity {1}", ParticipantStore.Participants[new Guid(id)], a.Name);
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
            return _useCloud && _connectionActive ? _activityCloudConnector.GetUsers(Owner.Id) : null;
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        /// <param name="deviceId"> </param>
        public void RequestFriendShip(string email, string deviceId)
        {
            _activityCloudConnector.RequestFriendShip(Owner.Id, _activityCloudConnector.GetIdFromUserEmail(email));
            Console.WriteLine("ActivityManager: Requested friendship to {0}", email);
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        /// <param name="deviceId"> </param>
        public void RemoveFriend(Guid friendId, string deviceId)
        {
            _activityCloudConnector.RemoveFriend(Owner.Id, friendId);
            Console.WriteLine("ActivityManager: Removed friendship to {0}", friendId);
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        /// <param name="deviceId"> </param>
        public void RespondToFriendRequest(Guid friendId, bool approval, string deviceId)
        {
            _activityCloudConnector.RespondToFriendRequest(Owner.Id, friendId,approval);
            Console.WriteLine("ActivityManager: Approved friendship to {0}? -> {1}", friendId,approval);
        }

        #endregion

        #region Pub/Sub
        /// <summary>
        /// Registers a device to the manager
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>A connection id</returns>
        public Guid Register(Device device)
        {
            var cc = new ConnectedClient(device.Name, device.BaseAddress, device);
            if (Registry.ConnectedClients.ContainsKey(device.Id.ToString()))
                return new Guid("null");
            Registry.ConnectedClients.Add(device.Id.ToString(), cc);
            _publisher.Publish( DeviceEvent.DeviceAdded.ToString(), device);
            Console.WriteLine("ActivityManager: Published {0}: {1}", EventType.DeviceEvents, DeviceEvent.DeviceAdded);
            //SendCache(device.Id.ToString());
            return device.Id;
        }
        private void SendCache(string deviceId)
        {

        }
        
        /// <summary>
        /// Unregisters a device from the service
        /// </summary>
        /// <param name="id">Connection id</param>
        public void UnRegister(string id)
        {
            if(id != null)
                if(Registry.ConnectedClients.ContainsKey(id))
                {
                    Registry.ConnectedClients.Remove(id);
                    _publisher.Publish( DeviceEvent.DeviceRemoved.ToString(), id,true);
                    Console.WriteLine("ActivityManager: Published {0}: {1}", EventType.DeviceEvents, DeviceEvent.DeviceAdded);
                }
        }
        #endregion

        #region Messenger
        public void SendMessage( string message,string deviceId)
        {
            _publisher.Publish( ComEvent.MessageReceived.ToString(), message);
        }
        #endregion
    }
}
