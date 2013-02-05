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
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using NooSphere.ActivitySystem.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NooSphere.Core.ActivityModel;
using Microsoft.AspNet.SignalR.Client;
#if ANDROID
#else
#endif

namespace NooSphere.ActivitySystem.Base.Service
{
    public class ActivityCloudConnector
    {
        #region Private Members
        private Connection _connection;
#if ANDROID
#else
        private readonly HttpClient _httpClient = new HttpClient();
#endif
        #endregion

        public string BaseUrl { get; set; }
        public string ConnectionId
        {
            get { return _connection.ConnectionId; }
        }

        #region Events
        public event EventHandler ConnectionSetup;

        public event ActivityAddedHandler ActivityAdded;
        public event ActivityChangedHandler ActivityUpdated;
        public event ActivityRemovedHandler ActivityDeleted;

        public event FileDownloadRequestHandler FileDownloadRequest;
        public event FileUploadRequestHandler FileUploadRequest;
        public event FileDeleteRequestHandler FileDeleteRequest;

        public event FriendAddedHandler FriendAdded;
        public event FriendDeletedHandler FriendDeleted;
        public event FriendRequestReceivedHandler FriendRequestReceived;

        public event MessageReceivedHandler MessageReceived;

        public event ParticipantAddedHandler ParticipantAdded;
        public event ParticipantRemovedHandler ParticipantRemoved;

        public event EventHandler UserOnline;
        public event EventHandler UserOffline;

        #endregion

        #region Constructor
        public ActivityCloudConnector()
        {


        }
        public void ConnectToCloud(string baseUrl,User user)
        {
            BaseUrl = baseUrl;
            _connection = new Connection(baseUrl+ "Connect");
           Connect(user);
        }
        ~ActivityCloudConnector()
        {
            Disconnect();
        }
        #endregion

        #region Public Members
        public void AddParticipant(Guid activityId, Guid userId)
        {
            Rest.Post(BaseUrl + "Activities/" + activityId + "/Participants/" + userId, null, _connection.ConnectionId);
        }
        public void RemoveParticipant(Guid activityId, Guid userId)
        {
            Rest.Delete(BaseUrl + "Activities/" + activityId + "/Participants/" + userId, null, _connection.ConnectionId);
        }
        public void Register(Guid userId)
        {
            Rest.Post(BaseUrl + "Users/" + userId + "/Device", null, _connection.ConnectionId);
        }
        public void Unregister(Guid userId)
        {
            Rest.Delete(BaseUrl + "Users/" + userId + "/Device", null, _connection.ConnectionId);
        }
        public List<Activity> GetActivities()
        {
            var result = Rest.Get(BaseUrl + "Activities", null, _connection.ConnectionId);
            return JsonConvert.DeserializeObject<List<Activity>>(result);
        }
        public Activity GetActivity(Guid activityId)
        {
            return JsonConvert.DeserializeObject<Activity>(
                Rest.Get(BaseUrl + "Activities/" + activityId, null, _connection.ConnectionId));
        }
        public void AddActivity(Activity activity)
        {
            Rest.Post(BaseUrl + "Activities/", activity, _connection.ConnectionId);
        }
        public void UpdateActivity(Activity activity)
        {
            Task.Factory.StartNew(
                delegate
                {

                        Rest.Put(BaseUrl + "Activities/" + activity.Id, activity,
                                 _connection.ConnectionId);
                        Console.WriteLine("UPDATE TO CLOUD========================");
                    });
        }
        public void DeleteActivity(Guid activityId)
        {
            Rest.Delete(BaseUrl + "Activities/" + activityId, null,
                                _connection.ConnectionId);;
        }
        public Guid GetIdFromUserEmail(string email)
        {
            return JsonConvert.DeserializeObject<User>(Rest.Get(BaseUrl + "Users?email=" + email)).Id;
        }
        public List<User> GetUsers(Guid userId)
        {
           var bare =Rest.Get(BaseUrl + "Users/"+userId+"/Friends/", null, _connection.ConnectionId);
           var res=JsonConvert.DeserializeObject<List<User>>(bare);
           return res;
        }
        public void AddResource(Resource resource, string localPath)
        {
            var path = BaseUrl + resource.CloudPath + "?size=" +
                                resource.Size.ToString(CultureInfo.InvariantCulture) + "&creationTime=" +
                                resource.CreationTime
                                + "&lastWriteTime=" + resource.LastWriteTime + "&relativePath=" +
                                HttpUtility.UrlEncode(resource.RelativePath);
            Rest.UploadStream(path, localPath, ConnectionId).ContinueWith(r => Log.Out("ActivityCloudConnector", string.Format("Finished upload of {0}", resource.Name), LogCode.Log));
            Log.Out("ActivityCloudConnector", string.Format("Started upload of {0}", resource.Name), LogCode.Log);
        }

        public void DeleteFile(Resource resource)
        {
            //File.Delete(resource.RelativePath);
        }
        public void RequestFriendShip(Guid userId,Guid friendId)
        {
            Rest.Post(BaseUrl + "Users/" + userId + "/Friends/" + friendId, null,
                                _connection.ConnectionId);
        }
        public void RemoveFriend(Guid userId,Guid friendId)
        {
            Rest.Delete(BaseUrl + "Users/" + userId + "/Friends/" + friendId, null,
                                _connection.ConnectionId);
        }
        public void RespondToFriendRequest(Guid userId,Guid friendId,bool approve)
        {
            Rest.Post(BaseUrl + "Users/" + userId + "/Friends/" + friendId+"?approve="+approve, null, _connection.ConnectionId);
        }
        #endregion

        #region Private Members
        private void Connect(User user)
        {
            _connection.Received += SignalRecieved;
            _connection.Start().Wait();
            Register(user.Id);
            if (ConnectionSetup != null)
                ConnectionSetup(this, new EventArgs());
            //_connection.Start().ContinueWith(task =>
            //{
            //    if (task.IsFaulted)
            //    {
            //        if (task.Exception != null)
            //            Console.WriteLine("Failed to start: {0}", task.Exception.InnerException.ToString());
            //    }
            //    else
            //    {
            //        Register(user.Id);
            //        if (ConnectionSetup != null)
            //            ConnectionSetup(this, new EventArgs());
            //    }
            //});
        }
        private void Disconnect()
        {
            if(_connection != null)
                _connection.Stop();
        }

        private void SignalRecieved(string obj)
        {
            if (obj == "Connected")
                return;
            var content = JsonConvert.DeserializeObject<JObject>(obj);
            var eventType = content["Event"].ToString();
            var data = content["Data"].ToObject<object>();


                        switch (eventType)
                        {
                            case "ActivityAdded":
                                if (ActivityAdded != null)
                                    ActivityAdded(this,
                                                  new ActivityEventArgs(
                                                      JsonConvert.DeserializeObject<Activity>(data.ToString())));
                                break;
                            case "ActivityUpdated":
                                if (ActivityUpdated != null)
                                    ActivityUpdated(this,
                                                    new ActivityEventArgs(
                                                        JsonConvert.DeserializeObject<Activity>(data.ToString())));
                                break;
                            case "ActivityDeleted":
                                if (ActivityDeleted != null)
                                {
                                    ActivityDeleted(this, new
                                                              ActivityRemovedEventArgs(
                                                              new Guid(
                                                                  JsonConvert.DeserializeObject<JObject>(data.ToString())
                                                                      ["Id"].ToString())));
                                }
                                break;
                            case "FileUpload":
                                if (FileUploadRequest != null)
                                {
                                    FileUploadRequest(this,
                                                      new FileEventArgs(
                                                          JsonConvert.DeserializeObject<Resource>(data.ToString())));
                                }
                                break;
                            case "FileDownload":
                                if (FileDownloadRequest != null)
                                {
                                    FileDownloadRequest(this,
                                                        new FileEventArgs(
                                                            JsonConvert.DeserializeObject<Resource>(data.ToString())));
                                }
                                break;
                            case "FileDelete":
                                if (FileDeleteRequest != null)
                                {
                                    FileDeleteRequest(this,
                                                      new FileEventArgs(
                                                          JsonConvert.DeserializeObject<Resource>(data.ToString())));
                                }
                                break;
                            case "FriendAdded":
                                if (FriendAdded != null)
                                    FriendAdded(this,
                                                new FriendEventArgs(JsonConvert.DeserializeObject<User>(data.ToString())));
                                break;
                            case "FriendDeleted":
                                if (FriendDeleted != null)
                                    FriendDeleted(this,
                                                  new FriendDeletedEventArgs(
                                                      JsonConvert.DeserializeObject<Guid>(data.ToString())));
                                break;
                            case "FriendRequest":
                                if (FriendRequestReceived != null)
                                    FriendRequestReceived(this,
                                                          new FriendEventArgs(
                                                              JsonConvert.DeserializeObject<User>(data.ToString())));
                                break;
                            case "Message":
                                if (MessageReceived != null)
                                    MessageReceived(this,
                                                    new ComEventArgs(
                                                        JsonConvert.DeserializeObject<Message>(data.ToString())));
                                break;
                            case "ParticipantAdded":
                                if (ParticipantAdded != null)
                                {
                                    var res = JsonConvert.DeserializeObject<JObject>(data.ToString());
                                    ParticipantAdded(this, new
                                                               ParticipantEventArgs(
                                                               res["Participant"].ToObject<User>(),
                                                               res["ActivityId"].ToObject<Guid>()));
                                }
                                break;
                            case "ParticipantRemoved":
                                if (ParticipantRemoved != null)
                                {
                                    var res = JsonConvert.DeserializeObject<JObject>(data.ToString());
                                    ParticipantRemoved(this, new
                                                                 ParticipantEventArgs(
                                                                 res["Participant"].ToObject<User>(),
                                                                 res["ActivityId"].ToObject<Guid>()));
                                }
                                break;
                            case "UserOnline":
                                if (UserOnline != null)
                                    UserOnline(this, new EventArgs());
                                break;
                            case "UserOffline":
                                if (UserOffline != null)
                                    UserOffline(this, new EventArgs());
                                break;
                        }

        }

        #endregion
        }
}
