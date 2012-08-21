﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Helpers;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.FileServer;
#if !ANDROID
using NooSphere.ActivitySystem.Host;
#endif

namespace NooSphere.ActivitySystem.Base.Client
{
#if !ANDROID
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
#endif
    public class ActivityClient : NetEventHandler,IActivityNode
    {
        #region Events
        public event ConnectionEstablishedHandler ConnectionEstablished = null;
        public event InitializedHandler Initialized = null;
        public event ContextMessageReceivedHandler ContextMessageReceived = null;
        #endregion

        #region Private Members
#if !ANDROID
        private readonly GenericHost _callbackService = new GenericHost();
#endif
        private readonly ConcurrentDictionary<Guid, Activity> _activityBuffer = new ConcurrentDictionary<Guid, Activity>(); 
        private FileStore _fileStore;
        private bool _connected;
        private string _connectionId;
        #endregion

        #region Properties
        public string Ip { get; set; }
        public string ClientName { get; set; }
        public Device Device { get; private set; }
        public string ServiceAddress { get; set; }
        public User CurrentUser { get; set; }
        public string LocalPath { get { return _fileStore.BasePath; } }
        public Dictionary<string,Device> DeviceList { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="localFileDirectory">The local file directory of the client</param>
        /// <param name="d">The device </param>
        public ActivityClient(string localFileDirectory,Device d)
        {
            InitializeFileService(localFileDirectory);
            Device = d;
            OnInitializedEvent(new EventArgs());

            ActivityChanged += ActivityClientActivityChanged;
            ActivityRemoved += ActivityClientActivityRemoved;
        }
        #endregion

        #region Initializer
        /// <summary>
        /// Initializes the File Service
        /// </summary>
        /// <param name="localPath">Path where the file service stores files</param>
        private void InitializeFileService(string localPath)
        {
            _fileStore = new FileStore(localPath);
            _fileStore.FileCopied += FileServerFileCopied;
            Log.Out("ActivityClient", string.Format("FileStore initialized at {0}", _fileStore.BasePath), LogCode.Log);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Downloads a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private byte[] DownloadResource(Resource resource)
        {
            return Rest.DownloadFromHttpStream(ServiceAddress + Url.Files+ "/" + resource.ActivityId + "/" + resource.Id, resource.Size);
        }

        /// <summary>
        /// Updates activity with a resource
        /// </summary>
        /// <param name="res">The resource that is added to the activity</param>
        private void UpdateActivityWithResources(Resource res)
        {
            var act = GetActivity(res.ActivityId.ToString());
            act.Resources.Add(res);

            //Update the activity
            UpdateActivity(act);
        }

        /// <summary>
        /// Tests the connection to the service
        /// </summary>
        /// <param name="addr">The address of the service</param>
        /// <param name="reconnectAttempts">Number of times the client tries to reconnect </param>
        private bool TestConnection(string addr,int reconnectAttempts)
        {
            Log.Out("ActivityClient", string.Format("Attempt to connect to {0}", addr), LogCode.Net);
            bool res;
            var attempts = 0;
            do
            {
                ServiceAddress = addr;
                res = JsonConvert.DeserializeObject<bool>(Rest.Get(ServiceAddress));
                Log.Out("ActivityClient", string.Format("Service active? -> {0}", res), LogCode.Net);
                Thread.Sleep(200);
                attempts++;
            }
            while (res == false && attempts < reconnectAttempts);
            if (res)
                OnConnectionEstablishedEvent(new EventArgs());
            else
                throw new Exception("ActivityClient: Could not connect to: " + addr);
            return true;
        }

        /// <summary>
        /// Register a given device with the activity client
        /// </summary>
        /// <param name="d">The device that needs to be registered with the activity client</param>
        private void Register(Device d)
        {
            if (!_connected)
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
#if ANDROID
            d.BaseAddress = BaseUrl;
#endif
#if !ANDROID
            d.BaseAddress = Net.GetUrl(Net.GetIp(IPType.All), StartCallbackService(), "").ToString();
#endif
            _connectionId = JsonConvert.DeserializeObject<String>(Rest.Post(ServiceAddress + Url.Devices, d));
            Log.Out("ActivityClient", string.Format("Received device id: " + _connectionId), LogCode.Log);
        }

        /// <summary>
        /// Starts a callback service. The activity manager uses this service to publish
        /// events.
        /// </summary>
        /// <returns>The port of the deployed service</returns>
#if !ANDROID
        private int StartCallbackService()
        {
            try
            {
                _callbackService.Open(this, typeof(INetEvent), "CallbackService");
                Log.Out("ActivityClient", string.Format("Callback service initialized at {0}", Net.GetUrl(Ip, Net.FindPort(), "")), LogCode.Log);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.ToString());
            }
            return _callbackService.Port;
        }
#endif
        #endregion

        #region Public Methods

        /// <summary>
        /// Connects the client to the activity service
        /// </summary>
        /// <param name="address">The address of the service</param>
        public void Open(string address)
        {
            //Automatically fetch a local IP
            Ip = Net.GetIp(IPType.All);

            //Test if connected to manager. Exception is thrown if not
            TestConnection(address, 25);

            //Listen to some of the internal events
            FileUploadRequest += ActivityClientFileUploadRequest;
            FileDownloadRequest += ActivityClientFileDownloadRequest;
            DeviceAdded += ActivityClientDeviceAdded;
            DeviceRemoved += ActivityClientDeviceRemoved;

            //Set connected flag true
            _connected = true;

            //Register this device with the manager
            Register(Device);
        }

        /// <summary>
        /// Unregister main device from the activity client
        /// </summary>
        public void Close()
        {
            if (_connected)
                Rest.Delete(ServiceAddress + Url.Devices + "/" + _connectionId);
            else
            {
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
            }
        }

        /// <summary>
        /// Sends an "add activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        /// <remarks>
        /// Before we can use the activity, the client needs to wait for the manager
        /// to publish the activity back to us, so the transaction is confirmed
        /// </remarks>
        public void AddActivity(Activity act)
        {
            //If we are connected to a manager, post an activityAdd request with the
            //given activity
            if (_connected)
            {
                //The activity manager is expecting a tuple={activity,deviceId}
                Rest.Post(ServiceAddress + Url.Activities,
                          new
                            {
                                act,
                                deviceId = _connectionId
                            });
            }
            else
                //Throw an error if we are not connected
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");

            //Wait for an ActivityAdded net event send by the manager
        }

        /// <summary>
        /// Handles activities that are published by the activity manager
        /// </summary>
        /// <param name="act">The activity that is published by the activity manager</param>
        private void HandleActivity(Activity act)
        {
            //In case there are resources, initialize the file server
            _fileStore.IntializePath(act.Id);

            //Add the activity to a local buffer
            _activityBuffer.AddOrUpdate(act.Id, act,(key, oldValue)=> act);
        }

        /// <summary>
        /// Adds a file to a given activity
        /// </summary>
        /// <param name="fileInfo">The fileinfo describing the file</param>
        /// <param name="activityId">The id of activity</param>
        public void AddResource(FileInfo fileInfo,Guid activityId)
        {
            //Create a new resource from the file
            var resource = new Resource(fileInfo.FullName,(int)fileInfo.Length, fileInfo.Name)
            {
                ActivityId = activityId,
                CreationTime = DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture),
                LastWriteTime = DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture)                   
            };
            var req = new FileRequest
                          {
                              Resouce = resource,
                              Bytes = JsonConvert.SerializeObject(File.ReadAllBytes(resource.FilePath))
                          };

            Rest.SendRequest(ServiceAddress + Url.Files, HttpMethod.Post, req);
            Log.Out("ActivityClient", string.Format("Received Request to upload {0}", resource.Name), LogCode.Log);

        }

        /// <summary>
        /// Handles the fileCopied event from the filestore
        /// </summary>
        /// <param name="e"></param>
        private void HandleFileServerFileCopied(FileEventArgs e)
        {
            //File is in the store,
            UploadResource(e.Resource);
        }

        /// <summary>
        /// Uploads a resource to the activity manager
        /// </summary>
        /// <param name="r"></param>
        private void UploadResource(Resource r)
        {
            Rest.SendStreamingRequest(ServiceAddress + "Files/" + r.ActivityId + "/" + r.Id, _fileStore.BasePath + r.RelativePath);
            //_fileServer.BasePath + r.RelativePath);
            Log.Out("ActivityClient", string.Format("Received Request to upload {0}", r.Name), LogCode.Log);
        }

        /// <summary>
        /// Sends a "Remove activity" request to the activity manager
        /// </summary>
        /// <param name="activityId">The id (of the activity) that needs to be included in the request</param>
        public void RemoveActivity(Guid activityId)
        {
            if(_connected)
                Rest.Delete(ServiceAddress + Url.Activities, new { activityId, deviceId=_connectionId });
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Sends an "Update activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void UpdateActivity(Activity act)
        {
           if(_connected)
                Rest.Put(ServiceAddress + Url.Activities, new { act, deviceId = _connectionId });
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Sends a "Get Activities" request to the activity manager
        /// </summary>
        /// <returns>A list of retrieved activities</returns>
        public List<Activity> GetActivities()
        {
            if (_connected)
            {
                var res = Rest.Get(ServiceAddress + Url.Activities);
                return JsonConvert.DeserializeObject<List<Activity>>(res);
            }
            throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Sends a "Get Activity" request to the activity manager
        /// </summary>
        /// <param name="activityId">The id (of the activity) that needs to be included in the request</param>
        /// <returns></returns>
        public Activity GetActivity(string activityId)
        {
            if(_connected)
                return JsonConvert.DeserializeObject<Activity>(Rest.Get(ServiceAddress + Url.Activities + "/" + activityId));
            throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Sends a "Send Message" request to the activity manager
        /// </summary>
        /// <param name="msg">The message that needs to be included in the request</param>
        public void SendMessage(string msg)
        {
            if(_connected)
                Rest.Post(ServiceAddress + Url.Messages, new { message = msg, deviceId=_connectionId });
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Gets all users in the friendlist
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <returns>A list with all users in the friendlist</returns>
        public List<User> GetUsers()
        {
            if(_connected)
                return JsonConvert.DeserializeObject<List<User>>(Rest.Get(ServiceAddress + Url.Users));
            throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            if(_connected)
                JsonConvert.DeserializeObject<List<User>>(Rest.Post(ServiceAddress + Url.Users, new { email, deviceId=_connectionId }));
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            if(_connected)
                JsonConvert.DeserializeObject<List<User>>(Rest.Delete(ServiceAddress + Url.Users, new { friendId, deviceId=_connectionId }));
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            if(_connected)
                Rest.Put(ServiceAddress + Url.Users, new{friendId, approval, deviceId=_connectionId});
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }

        #endregion

        #region Internal Event Handlers
        protected void OnContextReceivedEvent(ContextEventArgs e)
        {
            if (ContextMessageReceived != null)
                ContextMessageReceived(this, e);
        }
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

        #region Overrides
        public override void ActivityNetAdded(Activity act)
        {
            HandleActivity(act);

            base.ActivityNetAdded(act);
        }
        public override void ActivityNetRemoved(Guid id)
        {
            _fileStore.CleanUp(id.ToString());

            base.ActivityNetRemoved(id);
        }
        #endregion

        #region Event Handlers

        private void FileServerFileCopied(object sender, FileEventArgs e)
        {
            HandleFileServerFileCopied(e);
        }

        private void FileServerFileAdded(object sender, FileEventArgs e)
        {
            HandleFileServerFileCopied(e);
        }
        private void ActivityClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            Activity removedActivity;
            _activityBuffer.TryRemove(e.Id, out removedActivity);
        }
        private void ActivityClientActivityChanged(object sender, ActivityEventArgs e)
        {
            _activityBuffer[e.Activity.Id] = e.Activity;
        }

        private void ActivityClientFileDownloadRequest(object sender, FileEventArgs e)
        {
            _fileStore.AddFile(e.Resource, DownloadResource(e.Resource), FileSource.ActivityManager);
        }
        private void ActivityClientDeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            if (DeviceList != null)
                DeviceList.Remove(e.Id);
        }
        private void ActivityClientDeviceAdded(object sender, DeviceEventArgs e)
        {
            if (DeviceList == null)
                DeviceList = new Dictionary<string, Device>();
            DeviceList.Add(e.Device.Id.ToString(), e.Device);
        }
        private void ActivityClientFileUploadRequest(object sender, FileEventArgs e)
        {
            UploadResource(e.Resource);
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
