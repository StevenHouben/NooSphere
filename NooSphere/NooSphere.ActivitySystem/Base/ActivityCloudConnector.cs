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
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading;
using System.Web;

using SignalR.Client;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using NooSphere.Core.ActivityModel;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem
{
    public class ActivityCloudConnector
    {
        #region Private Members
        private string baseUrl;
        private Connection connection;
        #endregion

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
        public ActivityCloudConnector(string baseUrl,User user)
        {
            this.baseUrl = baseUrl;
            this.connection = new Connection(baseUrl + "Connect");
            Connect(user);
        }
        ~ActivityCloudConnector()
        {
            this.Disconnect();
        }
        #endregion

        #region Public Members
        public void AddParticipant(Guid activityId, Guid userId)
        {
            Rest.SendRequest(baseUrl + "Activities/" + activityId + "/Participants/" + userId, HttpMethod.Post, null, connection.ConnectionId);
        }
        public void RemoveParticipant(Guid activityId, Guid userId)
        {
            Rest.SendRequest(baseUrl + "Activities/" + activityId + "/Participants/" + userId, HttpMethod.Delete, null, connection.ConnectionId);
        }
        public void Register(Guid userId)
        {
            Rest.SendRequest(baseUrl + "Users/" + userId + "/Device", HttpMethod.Post, null, connection.ConnectionId);
        }
        public void Unregister(Guid userId)
        {
            Rest.SendRequest(baseUrl + "Users/" + userId + "/Device", HttpMethod.Delete, null, connection.ConnectionId);
        }
        public List<Activity> GetActivities()
        {
            return JsonConvert.DeserializeObject<List<Activity>>(Rest.SendRequest(baseUrl + "Activities", HttpMethod.Get, null, connection.ConnectionId));
        }
        public Activity GetActivity(Guid activityId)
        {
            return JsonConvert.DeserializeObject<Activity>(Rest.SendRequest(baseUrl + "Activities/" + activityId, HttpMethod.Get, null, connection.ConnectionId));
        }
        public void AddActivity(Activity activity)
        {
            Rest.SendRequest(baseUrl + "Activities/", HttpMethod.Post, activity, connection.ConnectionId);
        }
        public void UpdateActivity(Activity activity)
        {
            Rest.SendRequest(baseUrl + "Activities/" + activity.Id, HttpMethod.Put, activity, connection.ConnectionId);
        }
        public void DeleteActivity(Guid activityId)
        {
            Rest.SendRequest(baseUrl + "Activities/" + activityId, HttpMethod.Delete, null, connection.ConnectionId);
        }
        public Guid GetIdFromUserEmail(string email)
        {
            return JsonConvert.DeserializeObject<User>(Rest.Get(baseUrl + "Users?email=" + email)).Id;
        }
        public List<User> GetUsers(Guid userId)
        {
           var bare =Rest.SendRequest(baseUrl + "Users/"+userId+"/Friends/", HttpMethod.Get, null, connection.ConnectionId);
           var res=JsonConvert.DeserializeObject<List<User>>(bare);
           return res;
        }
        public byte[] GetResource(Resource resource)
        {
            return JsonConvert.DeserializeObject<byte[]>( Rest.SendRequest(baseUrl + Id(resource.ActivityId, resource.ActionId, 
                resource.Id), HttpMethod.Get, null, connection.ConnectionId));
        }
        public void AddResource(Resource resource,byte[] buffer)
        {
            Rest.SendRequest(baseUrl + Id(resource.ActivityId, resource.ActionId, resource.Id) + "?size=" + resource.Size.ToString() + "&creationTime=" + resource.CreationTime
            + "&lastWriteTime=" + resource.LastWriteTime + "&relativePath=" + HttpUtility.UrlEncode(resource.RelativePath), HttpMethod.Post, buffer, connection.ConnectionId);
        }
        public void DeleteFile(Resource resource)
        {
            //File.Delete(resource.RelativePath);
        }
        public void RequestFriendShip(Guid userId,Guid friendId)
        {
            Rest.SendRequest(baseUrl + "Users/" + userId + "/Friends/" + friendId, HttpMethod.Post, null, connection.ConnectionId);
        }
        public void RemoveFriend(Guid userId,Guid friendId)
        {
            Rest.SendRequest(baseUrl + "Users/" + userId + "/Friends/" + friendId, HttpMethod.Delete, null, connection.ConnectionId);
        }
        public void RespondToFriendRequest(Guid userId,Guid friendId,bool approve)
        {
            Rest.SendRequest(baseUrl + "Users/" + userId + "/Friends/" + friendId+"?approve="+approve, HttpMethod.Post, null, connection.ConnectionId);
        }
        #endregion

        #region Private Members
        private void Connect(User user)
        {
            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Console.WriteLine("Failed to start: {0}", task.Exception.GetBaseException());
                else
                {
                    Register(user.Id);
                    if (ConnectionSetup != null)
                        ConnectionSetup(this, new EventArgs());
                    connection.Received += SignalRecieved;
                }
            });
        }
        private void Disconnect()
        {
            if(connection != null)
                connection.Stop();
        }
       
        private void SignalRecieved(string obj)
        {
            if (obj == "Connected")
                return;
            JObject content = JsonConvert.DeserializeObject<JObject>(obj);
            string eventType = content["Event"].ToString();
            object data = content["Data"].ToObject<object>();

            Thread t = new Thread(()=>
            {
                switch (eventType)
                {
                    case "ActivityAdded":
                        if (ActivityAdded != null)
                            ActivityAdded(this, 
                                new ActivityEventArgs(JsonConvert.DeserializeObject<Activity>(data.ToString())));
                        break;
                    case "ActivityUpdated":
                        if (ActivityUpdated != null)
                            ActivityUpdated(this, 
                                new ActivityEventArgs(JsonConvert.DeserializeObject<Activity>(data.ToString())));
                        break;
                    case "ActivityDeleted":
                        if (ActivityDeleted != null)
                        {
                            ActivityDeleted(this, new
                                ActivityRemovedEventArgs(
                                new Guid(JsonConvert.DeserializeObject<JObject>(data.ToString())["Id"].ToString())));
                        }
                        break;
                    case "FileUpload":
                        if (FileUploadRequest != null)
                        {
                            FileUploadRequest(this, 
                                new FileEventArgs(JsonConvert.DeserializeObject<Resource>(data.ToString())));
                            //new Thread(() => UploadFile(((JObject)data).ToObject<Resource>())).Start();
                        }
                        break;
                    case "FileDownload":
                        if (FileDownloadRequest != null)
                        {
                            FileDownloadRequest(this,
                                new FileEventArgs(JsonConvert.DeserializeObject<Resource>(data.ToString())));
                            //new Thread(() => DownloadFile(((JObject)data).ToObject<Resource>())).Start();
                        }
                        break;
                    case "FileDelete":
                        if (FileDeleteRequest != null)
                        {
                            FileDeleteRequest(this,
                                new FileEventArgs(JsonConvert.DeserializeObject<Resource>(data.ToString())));
                            //new Thread(() => DeleteFile(((JObject)data).ToObject<Resource>())).Start();
                        }
                        break;
                    case "FriendAdded":
                        if(FriendAdded != null)
                            FriendAdded(this,new FriendEventArgs( JsonConvert.DeserializeObject<User>(data.ToString())));
                        break;
                    case "FriendDeleted":
                        if (FriendDeleted != null)
                            FriendDeleted(this,new FriendDeletedEventArgs(JsonConvert.DeserializeObject<Guid>(data.ToString())));
                        break;
                    case "FriendRequest":
                        if (FriendRequestReceived != null)
                            FriendRequestReceived(this,new FriendEventArgs( JsonConvert.DeserializeObject<User>(data.ToString())));
                        break;
                    case "Message":
                        if(MessageReceived != null)
                            MessageReceived(this,new ComEventArgs(JsonConvert.DeserializeObject<String>(data.ToString())));
                        break;
                    case "ParticipantAdded":
                        if (ParticipantAdded != null)
                        {
                            JObject res = JsonConvert.DeserializeObject<JObject>(data.ToString());
                            ParticipantAdded(this, new
                                ParticipantEventArgs(res["Participant"].ToObject<User>(),res["ActivityId"].ToObject<Guid>()));
                        }
                        break;
                    case "ParticipantRemoved":
                        if (ParticipantRemoved != null)
                        {
                            JObject res = JsonConvert.DeserializeObject<JObject>(data.ToString());
                            ParticipantRemoved(this, new
                                ParticipantEventArgs(res["Participant"].ToObject<User>(), res["ActivityId"].ToObject<Guid>()));
                        }
                        break;
                    //case "UserOnline":
                    //    if (UserOnline != null)
                    //        UserOnline(this, new DataEventArgs(data));
                    //    break;
                    //case "UserOffline":
                    //    if (UserOffline != null)
                    //        UserOffline(this, new DataEventArgs(data));
                        break;
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        private string Id(Guid activityId, Guid actionId, Guid resourceId)
        {
            return "Activities/" + activityId + "/Actions/" + actionId + "/Resources/" + resourceId;
        }
        #endregion
    }
}
