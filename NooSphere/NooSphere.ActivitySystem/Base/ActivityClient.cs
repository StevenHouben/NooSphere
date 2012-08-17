/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

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
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.Context;

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
        private readonly GenericHost _callbackService = new GenericHost();
        private FileService _fileServer;
        private bool _connected;
        private string _connectionId;
        private MulticastSocket _mSocket;
        #endregion

        #region Properties
        public string Ip { get; set; }
        public string ClientName { get; set; }
        public Device Device { get; private set; }
        public string ServiceAddress { get; set; }
        public User CurrentUser { get; set; }
        public string LocalPath { get { return _fileServer.BasePath; } }
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

            ActivityAdded += ActivityClientActivityAdded;
        }

        void ActivityClientActivityAdded(object sender, ActivityEventArgs e)
        {
            _fileServer.IntializePath(e.Activity);
        }

        /// <summary>
        /// Initializes the File Service
        /// </summary>
        /// <param name="localPath">Path where the file service stores files</param>
        private void InitializeFileService(string localPath)
        {
            _fileServer = new FileService(localPath);
            _fileServer.FileAdded += FileServerFileAdded;
            _fileServer.FileCopied += FileServerFileCopied;
            //_fileServer.FileChanged += FileServerFileChanged;
            //_fileServer.FileRemoved += FileServerFileRemoved;
            Log.Out("ActivityClient", string.Format("FileStore initialized at {0}", _fileServer.BasePath), LogCode.Log);
        }

        #endregion

        #region Private Methods
        private void UploadResource(Resource r)
        {
            Rest.SendStreamingRequest(ServiceAddress + "Files/" + r.ActivityId + "/" + r.Id, _fileServer.BasePath+ r.RelativePath);
                                    //_fileServer.BasePath + r.RelativePath);
            Log.Out("ActivityClient", string.Format("Received Request to upload {0}", r.Name), LogCode.Log);
        }
        /// <summary>
        /// Tests the connection to the service
        /// </summary>
        /// <param name="addr">The address of the service</param>
        private void TestConnection(string addr)
        {
            Log.Out("ActivityClient", string.Format("Attempt to connect to {0}", addr), LogCode.Net);
            bool res;
            var attempts = 0;
            const int maxAttemps = 20;
            do
            {
                ServiceAddress = addr;
                res = JsonConvert.DeserializeObject<bool>(Rest.Get(ServiceAddress));
                Log.Out("ActivityClient", string.Format("Service active? -> {0}", res), LogCode.Net);
                Thread.Sleep(100);
                attempts++;
            }
            while (res == false && attempts<maxAttemps);
            if (res)
                OnConnectionEstablishedEvent(new EventArgs());
            else
                throw new Exception("ActivityClient: Could not connect to: " + addr);
        }

        /// <summary>
        /// Register a given device with the activity client
        /// </summary>
        /// <param name="d">The device that needs to be registered with the activity client</param>
        private void Register(Device d)
        {
            if (!_connected)
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
            d.BaseAddress = Net.GetUrl(Net.GetIp(IPType.All), StartCallbackService(), "").ToString();
            _connectionId = JsonConvert.DeserializeObject<String>(Rest.Post(ServiceAddress + Url.Devices, d));
            Log.Out("ActivityClient", string.Format("Received device id: " + _connectionId), LogCode.Log);
        }

        /// <summary>
        /// Starts a callback service. The activity manager uses this service to publish
        /// events.
        /// </summary>
        /// <returns>The port of the deployed service</returns>
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

        /// <summary>
        /// Initialize the UDP context pipeline
        /// </summary>
        private void IntializeContext()
        {
            _mSocket = new MulticastSocket("224.10.10.10", 33333, 0);
            _mSocket.OnNotifyMulticastSocketListener += _mSocket_OnNotifyMulticastSocketListener;
            _mSocket.StartReceiving();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Connects the client to the activity service
        /// </summary>
        /// <param name="address">The address of the service</param>
        public void Open(string address)
        {
            Ip = Net.GetIp(IPType.All);
            TestConnection(address);
            FileUploadRequest += ActivityClientFileUploadRequest;
            FileDownloadRequest += ActivityClient_FileDownloadRequest;
            DeviceAdded += ActivityClientDeviceAdded;
            DeviceRemoved += ActivityClientDeviceRemoved;
            _connected = true;
            Register(Device);

            IntializeContext();
        }

        private byte[] DownloadResource(Resource resource)
        {
            return Rest.DownloadFromHttpStream(ServiceAddress + "Files/" + resource.ActivityId + "/" + resource.Id,resource.Size);
        }

        /// <summary>
        /// Sends a context update to the multicast group
        /// </summary>
        /// <param name="context">Context message</param>
        public void SendContext(string context)
        {
            _mSocket.Send(context);
        }

        /// <summary>
        /// Unregister main device from the activity client
        /// </summary>
        public void Close()
        {
            if(_connected)
                Rest.Delete(ServiceAddress + Url.Devices, _connectionId);
            else
            {
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
            }
        }

        /// <summary>
        /// Sends an "add activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void AddActivity(Activity act)
        {
            if(_connected)
                Rest.Post(ServiceAddress + Url.Activities, new {act,deviceId=_connectionId});
            else
                throw new Exception("ActivityClient: Not connected to service. Call connect() method or check address");
        }
        public void AddResource(FileInfo fileInfo,Activity activity)
        {
            //create a new resource from the file
            var resource = new Resource(fileInfo.FullName,(int)fileInfo.Length, fileInfo.Name)
            {
                ActivityId = activity.Id,
                CreationTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                LastWriteTime = DateTime.Now.ToString(CultureInfo.InvariantCulture)                   
            };
            
            //Add the resource and file to the local file store as system
            _fileServer.AddFile(resource,File.ReadAllBytes(fileInfo.FullName),FileSource.ActivityClient);
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
        private void _mSocket_OnNotifyMulticastSocketListener(object sender, NotifyMulticastSocketListenerEventArgs e)
        {
            if (e.Type == MulticastSocketMessageType.MessageReceived)
                Console.WriteLine(System.Text.Encoding.ASCII.GetString((byte[])e.NewObject));
            //do stuff here
        }
        #endregion

        #region Event Handlers

        void FileServerFileCopied(object sender, FileEventArgs e)
        {
            //Couple the resource which is not in the filestore to the activity
            var act = GetActivity(e.Resource.ActivityId.ToString());
            act.Resources.Add(e.Resource);

            //Update the activity
            UpdateActivity(act);
        }
        void ActivityClient_FileDownloadRequest(object sender, FileEventArgs e)
        {
            _fileServer.AddFile(e.Resource, DownloadResource(e.Resource), FileSource.ActivityManager);
        }
        private void FileServerFileAdded(object sender, FileEventArgs e)
        {

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
