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
using System.IO;
using System.Net;
#if ANDROID
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
#if !ANDROID
using System.ServiceModel;
#endif
using NooSphere.Core.Devices;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.Base
{
    #if !ANDROID
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    #endif
    public class NetEventHandler : INetEvent
    {
        #if ANDROID
        #region Private Members
        private readonly HttpListener _httpListener;
        #endregion

        #region Protected Members
        protected string BaseUrl;
        #endregion

        #region Constructor
        public NetEventHandler()
        {
            BaseUrl = Net.GetUrl(Net.GetIp(IPType.All), Net.FindPort(), "").ToString();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(BaseUrl);

            _httpListener.Start();
            _httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpListener.BeginGetContext(HandleRequest, _httpListener);
        }
        #endregion

        #region HttpHandlers
        private void HandleRequest(IAsyncResult result)
        {
            var context = _httpListener.EndGetContext(result);
            var url = context.Request.RawUrl;
            var path = url.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            switch (context.Request.HttpMethod)
            {
                case "POST":
                    switch (path[0])
                    {
                        case "ActivityAdded":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                ActivityNetAdded(JsonConvert.DeserializeObject<Activity>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "ActivityRemoved":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                ActivityNetRemoved(JsonConvert.DeserializeObject<Guid>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "ActivityChanged":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                ActivityNetAdded(JsonConvert.DeserializeObject<Activity>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "MessageReceived":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                MessageNetReceived(JsonConvert.DeserializeObject<string[]>(streamReader.ReadToEnd())[0]);
                            Respond(context, 200, String.Empty);
                            break;
                        case "DeviceAdded":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                DeviceNetAdded(JsonConvert.DeserializeObject<Device>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "DeviceRemoved":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                DeviceNetRemoved(JsonConvert.DeserializeObject<string>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "DeviceRoleChanged":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                DeviceNetRoleChanged(JsonConvert.DeserializeObject<Device>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FileDownloadRequest":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FileNetDownloadRequest(JsonConvert.DeserializeObject<Resource>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FileDeleteRequest":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FileNetDeleteRequest(JsonConvert.DeserializeObject<Resource>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FileUploadRequest":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FileNetUploadRequest(JsonConvert.DeserializeObject<Resource>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FriendAdded":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FriendNetAdded(JsonConvert.DeserializeObject<User>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FriendRequest":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FriendNetRequest(JsonConvert.DeserializeObject<User>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "FriendRemoved":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                                FriendNetRemoved(JsonConvert.DeserializeObject<Guid>(streamReader.ReadToEnd()));
                            Respond(context, 200, String.Empty);
                            break;
                        case "ParticipantAdded":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                            {
                                var obj = JObject.Parse(streamReader.ReadToEnd());
                                ParticipantNetAdded(JsonConvert.DeserializeObject<User>(obj["u"].ToString()),
                                                    JsonConvert.DeserializeObject<Guid>(obj["activityId"].ToString()));
                            }
                            Respond(context, 200, String.Empty);
                            break;
                        case "ParticipantRemoved":
                            using (var streamReader = new StreamReader(context.Request.InputStream))
                            {
                                var obj = JObject.Parse(streamReader.ReadToEnd());
                                ParticipantNetRemoved(JsonConvert.DeserializeObject<User>(obj["u"].ToString()),
                                                      JsonConvert.DeserializeObject<Guid>(obj["activityId"].ToString()));
                            }
                            Respond(context, 200, String.Empty);
                            break;
                    }
                    break;
                case "GET":
                    if (path.Length == 0)
                        Respond(context, 200, "true");
                    else
                        HandleBadRequest(context);
                    break;
                default:
                    HandleBadRequest(context);
                    break;
            }
        }

        private void HandleBadRequest(HttpListenerContext context)
        {
            Respond(context, 400, "Bad request.");
        }

        private void Respond(HttpListenerContext context, int statusCode, string content)
        {
            context.Response.StatusCode = statusCode;
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            context.Response.Close();
            _httpListener.BeginGetContext(HandleRequest, _httpListener);
        }
        #endregion
        #endif

        #region Events
        public event ActivityAddedHandler ActivityAdded;
        public event ActivityRemovedHandler ActivityRemoved;
        public event ActivityChangedHandler ActivityChanged;

        public event DeviceAddedHandler DeviceAdded;
        public event DeviceRemovedHandler DeviceRemoved;
        public event DeviceRoleChangedHandler DeviceRoleChanged;

        public event MessageReceivedHandler MessageReceived;

        public event FileDownloadRequestHandler FileDownloadRequest;
        public event FileUploadRequestHandler FileUploadRequest;
        public event FileDeleteRequestHandler FileDeleteRequest;

        public event FriendAddedHandler FriendAdded;
        public event FriendDeletedHandler FriendDeleted;
        public event FriendRequestReceivedHandler FriendRequestReceived;

        public event ParticipantAddedHandler ParticipantAdded;
        public event ParticipantRemovedHandler ParticipantRemoved;

        public event EventHandler UserOnline;
        public event EventHandler UserOffline;

        #endregion

        #region Net Event handlers
        protected virtual void OnUserOffline(EventArgs e)
        {
            if (UserOffline != null) 
                UserOffline(this, e);
        }

        protected virtual void OnUserOnline(EventArgs e)
        {
            if (UserOnline != null) 
                UserOnline(this, e);
        }
        public virtual void ActivityNetAdded(Activity act)
        {
            if (ActivityAdded != null)
                ActivityAdded(this, new ActivityEventArgs(act));
        }
        public virtual void ActivityNetRemoved(Guid id)
        {
             if (ActivityRemoved != null)
                ActivityRemoved(this, new ActivityRemovedEventArgs(id));
        }
        public virtual void ActivityNetChanged(Activity act)
        {
             if (ActivityChanged != null)
                ActivityChanged(this, new ActivityEventArgs(act));
        }
        public virtual void MessageNetReceived(string msg)
        {
            if (MessageReceived != null)
                MessageReceived(this, new ComEventArgs(msg));
        }
        public virtual void FileNetDownloadRequest(Resource r)
        {
            if (FileDownloadRequest != null)
                FileDownloadRequest(this, new FileEventArgs(r));
        }
        public virtual void FileNetDeleteRequest(Resource r)
        {
            if (FileDeleteRequest != null)
                FileDeleteRequest(this, new FileEventArgs(r));
        }
        public virtual void FileNetUploadRequest(Resource r)
        {
            if (FileUploadRequest != null)
                FileUploadRequest(this, new FileEventArgs(r));
        }
        public virtual void DeviceNetAdded(Device dev)
        {
            if (DeviceAdded != null)
                DeviceAdded(this, new DeviceEventArgs(dev));
        }
        public virtual void DeviceNetRemoved(string id)
        {
            if (DeviceRemoved != null)
                DeviceRemoved(this, new DeviceRemovedEventArgs(id));
        }
        public virtual void DeviceNetRoleChanged(Core.Devices.Device dev)
        {
            if (DeviceRoleChanged != null)
                DeviceRoleChanged(this, new DeviceEventArgs(dev));
        }
        public virtual void FriendNetAdded(User u)
        {
            if(FriendAdded != null)
                FriendAdded(this,new FriendEventArgs(u));
        }

        public virtual void FriendNetRequest(User u)
        {
            if(FriendRequestReceived != null)
                FriendRequestReceived(this, new FriendEventArgs(u));
        }

        public virtual void FriendNetRemoved(Guid i)
        {
            if (FriendDeleted != null)
                FriendDeleted(this, new FriendDeletedEventArgs(i));
        }

        public virtual void ParticipantNetAdded(User u, Guid activityId)
        {
            if(ParticipantAdded != null)
                ParticipantAdded(this, new ParticipantEventArgs(u,activityId));
        }

        public virtual void ParticipantNetRemoved(User u, Guid activityId)
        {
            if (ParticipantRemoved != null)
                ParticipantRemoved(this, new ParticipantEventArgs(u, activityId));
        }

        public bool Alive()
        {
            return true;
        }
        #endregion
    }
}
