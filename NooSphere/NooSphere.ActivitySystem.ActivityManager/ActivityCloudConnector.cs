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

using NooSphere.ActivitySystem.ActivityService.ActivityManagement;
using NooSphere.Core.ActivityModel;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.ActivityService
{
    public class ActivityCloudConnector
    {
        #region Private Members
        private string baseUrl;
        private string baseDir;
        private Connection connection;
        #endregion

        #region Events
        public event EventHandler UserAdded;
        public event EventHandler UserUpdated;
        public event EventHandler UserConnected;
        public event EventHandler UserDisconnected;
        public event EventHandler UserDeleted;
        public event EventHandler ActivityAdded;
        public event EventHandler ActivityUpdated;
        public event EventHandler ActivityDeleted;
        #endregion

        #region Constructor
        public ActivityCloudConnector(string baseUrl, string baseDir, User user)
        {
            this.baseUrl = baseUrl;
            this.baseDir = baseDir;
            this.connection = new Connection(baseUrl + "Connect");
            Connect(user);
        }
        #endregion

        #region Public Members
        public List<User> GetUsers()
        {
            return JsonConvert.DeserializeObject<List<User>>(RestHelper.SendRequest(baseUrl + "Users", HttpMethod.Get, null, connection.ConnectionId));
        }
        public User GetUser(string userId)
        {
            return JsonConvert.DeserializeObject<User>(RestHelper.SendRequest(baseUrl + "Users/" + userId, HttpMethod.Get, null, connection.ConnectionId));
        }
        public void AddUser(User user)
        {
            RestHelper.SendRequest(baseUrl + "Users?connectionId=" + connection.ConnectionId, HttpMethod.Post, user);
        }
        public void UpdateUser(User user)
        {
            RestHelper.SendRequest(baseUrl + "Users/" + user.Id, HttpMethod.Put, user, connection.ConnectionId);
        }
        public void DeleteUser(string userId)
        {
            RestHelper.SendRequest(baseUrl + "Users/" + userId, HttpMethod.Delete, null, connection.ConnectionId);
        }
        public void Register(string userId)
        {
            RestHelper.SendRequest(baseUrl + "Users/" + userId + "/Device", HttpMethod.Post, null, connection.ConnectionId);
        }
        public void Unregister(string userId)
        {
            RestHelper.SendRequest(baseUrl + "Users/" + userId + "/Device", HttpMethod.Delete, null, connection.ConnectionId);
        }
        public List<Activity> GetActivities()
        {
            var result = RestHelper.SendRequest(baseUrl + "Activities", HttpMethod.Get, null, connection.ConnectionId);
            return JsonConvert.DeserializeObject<List<Activity>>(result);
        }
        public Activity GetActivity(string activityId)
        {
            return JsonConvert.DeserializeObject<Activity>(RestHelper.SendRequest(baseUrl + "Activities/" + activityId, HttpMethod.Get, null, connection.ConnectionId));
        }
        public void AddActivity(Activity activity)
        {
            RestHelper.SendRequest(baseUrl + "Activities/", HttpMethod.Post, activity, connection.ConnectionId);
        }
        public void UpdateActivity(Activity activity)
        {
            RestHelper.SendRequest(baseUrl + "Activities/" + activity.Id, HttpMethod.Put, activity, connection.ConnectionId);
        }
        public void DeleteActivity(string activityId)
        {
            RestHelper.SendRequest(baseUrl + "Activities/" + activityId, HttpMethod.Delete, null, connection.ConnectionId);
        }
        public void DeleteFile(Resource resource)
        {
            File.Delete(baseDir + resource.RelativePath);
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
                    Register(user.Id.ToString());
                    connection.Received += SignalRecieved;
                }
            });
            Console.ReadLine();
        }
        private void Disconnect()
        {
            connection.Stop();
        }
        private void DownloadFile(Resource resource)
        {
            string AbsolutePath = Path.Combine(baseDir, resource.RelativePath);

            FileStream fs = new FileStream(AbsolutePath, FileMode.Create);
            string result = RestHelper.SendRequest(baseUrl + Id(resource.ActivityId, resource.ActionId, resource.Id), HttpMethod.Get, null, connection.ConnectionId);
            byte[] bytestream = JsonConvert.DeserializeObject<byte[]>(result);
            fs.Write(bytestream, 0, resource.Size);
            fs.Close();

            File.SetCreationTimeUtc(AbsolutePath, DateTime.Parse(resource.CreationTime));
            File.SetLastWriteTimeUtc(AbsolutePath, DateTime.Parse(resource.LastWriteTime));
        }
        private void UploadFile(Resource resource)
        {
            FileInfo fi = new FileInfo(baseDir + resource.RelativePath);
            byte[] buffer = new byte[fi.Length];

            using (FileStream fs = new FileStream(Path.Combine(baseDir, resource.RelativePath), FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);

            RestHelper.SendRequest(baseUrl + Id(resource.ActivityId, resource.ActionId, resource.Id) + "?size=" + resource.Size.ToString() + "&creationTime=" + resource.CreationTime
                + "&lastWriteTime=" + resource.LastWriteTime + "&relativePath=" + HttpUtility.UrlEncode(resource.RelativePath), HttpMethod.Post, buffer, connection.ConnectionId);
        }
        private void SignalRecieved(string obj)
        {
            JObject content = JsonConvert.DeserializeObject<JObject>(obj);
            string eventType = content["Event"].ToString();
            object data = JsonConvert.DeserializeObject<object>(content["Data"].ToString());

            switch (eventType)
            {
                case "FileUpload":
                    new Thread(() => UploadFile(((JObject)data).ToObject<Resource>())).Start();
                    break;
                case "FileDownload":
                    new Thread(() => DownloadFile(((JObject)data).ToObject<Resource>())).Start();
                    break;
                case "FileDelete":
                    new Thread(() => DeleteFile(((JObject)data).ToObject<Resource>())).Start();
                    break;
                case "UserAdded":
                    if (UserAdded != null)
                        UserAdded(this, new DataEventArgs(data));
                    break;
                case "UserUpdated":
                    if (UserUpdated != null)
                        UserUpdated(this, new DataEventArgs(data));
                    break;
                case "UserDeleted":
                    if (UserDeleted != null)
                        UserDeleted(this, new DataEventArgs(data));
                    break;
                case "UserConnected":
                    if (UserConnected != null)
                        UserConnected(this, new DataEventArgs(data));
                    break;
                case "UserDisconnected":
                    if (UserDisconnected != null)
                        UserDisconnected(this, new DataEventArgs(data));
                    break;
                case "ActivityAdded":
                    if (ActivityAdded != null)
                        ActivityAdded(this, new DataEventArgs(data));
                    break;
                case "ActivityUpdated":
                    if (ActivityUpdated != null)
                        ActivityUpdated(this, new DataEventArgs(data));
                    break;
                case "ActivityDeleted":
                    if (ActivityDeleted != null)
                        ActivityDeleted(this, new DataEventArgs(data));
                    break;

            }
        }
        private string Id(Guid activityId, Guid actionId, Guid resourceId)
        {
            return "Activities/" + activityId + "/Actions/" + actionId + "/Resources/" + resourceId;
        }
        #endregion
    }
}
