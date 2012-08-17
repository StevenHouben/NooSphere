using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.Base;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Helpers;
using Activity = Android.App.Activity;
using Environment = System.Environment;

namespace ActivityDroid
{
    [Activity(Label = "Main")]
    public class Main : Activity
    {
        #region Properties

        private ActivityClient _client;
        private User _user;
        private Device _device;
        #endregion

        #region OnCreate
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var btnAddActivity = FindViewById<Button>(Resource.Id.AddActivity);
            btnAddActivity.Click += BtnAddActivity;

            SetUser(Intent.GetStringExtra("User"));
            SetStatus("Hi " + _user.Name + ", you are logged in.");
            StartActivityManager();
        }
        #endregion

        #region Private Methods
        private void BtnAddActivity(object sender, EventArgs e)
        {
            AddActivity(GetInitializedActivity());
        }
        private void SetStatus(string status)
        {
            RunOnUiThread(() => FindViewById<TextView>(Resource.Id.Status).Text = status);
        }

        private void SetUser(string json)
        {
            _user = JsonConvert.DeserializeObject<User>(json);
        }

        private void StartActivityManager()
        {
            _device = new Device
                          {
                              DeviceType = DeviceType.SmartPhone,
                              DevicePortability = DevicePortability.Mobile,
                              Name = Build.Device
                          };
            //_host = new Host(_user, new ContextWrapper(this).GetDir("ActivityCloud", FileCreationMode.Private));
            StartClient("http://10.1.1.190:57484/");
        }

        private void StartClient(string activityManagerHttpAddress)
        {
            var path = GetExternalFilesDir("ActivityCloud").AbsolutePath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            _client = new ActivityClient(path, _device) { CurrentUser = _user };

            _client.ActivityAdded += ClientActivityAdded;
            _client.ActivityChanged += ClientActivityChanged;
            _client.ActivityRemoved += ClientActivityRemoved;
            _client.MessageReceived += ClientMessageReceived;

            _client.FriendAdded += client_FriendAdded;
            _client.FriendDeleted += client_FriendDeleted;
            _client.FriendRequestReceived += ClientFriendRequestReceived;

            _client.FileUploadRequest += clientFileUploadRequest;
            _client.FileDownloadRequest += clientFileDownloadRequest;
            _client.FileDeleteRequest += clientFileDeleteRequest;

            _client.ConnectionEstablished += ClientConnectionEstablished;

            _client.Open(activityManagerHttpAddress);
        }

        private void AddActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            _client.AddActivity(activity);
        }
        #endregion

        #region Events

        private void ClientActivityAdded(object sender, ActivityEventArgs e)
        {
            Log.Out("Main", "Activity Added");
        }

        private void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            Log.Out("Main", "Activity Removed");
        }

        private void ClientActivityChanged(object sender, ActivityEventArgs e)
        {
            Log.Out("Main", "Activity Changed");
        }

        private void ClientMessageReceived(object sender, ComEventArgs e)
        {
            Log.Out("Main", "Message Received");
        }

        private void client_FriendAdded(object sender, FriendEventArgs e)
        {
            Log.Out("Main", "Friend Added");
        }

        private void client_FriendDeleted(object sender, FriendDeletedEventArgs e)
        {
            Log.Out("Main", "Friend Deleted");
        }

        private void ClientFriendRequestReceived(object sender, FriendEventArgs e)
        {
            Log.Out("Main", "Friend Request Received");
        }

        private void clientFileUploadRequest(object sender, FileEventArgs e)
        {
            Log.Out("Main", "File Upload Request");
        }

        private void clientFileDownloadRequest(object sender, FileEventArgs e)
        {
            Log.Out("Main", "File Download Request");
        }

        private void clientFileDeleteRequest(object sender, FileEventArgs e)
        {
            Log.Out("Main", "File Delete Request");
        }

        private void ClientConnectionEstablished(object sender, EventArgs e)
        {
            Log.Out("Main", "Connection Established");
        }
        #endregion

        #region Helper

        /// <summary>
        /// Generates a default activity
        /// </summary>
        /// <returns>An intialized activity</returns>
        public NooSphere.Core.ActivityModel.Activity GetInitializedActivity()
        {
            var ac = new NooSphere.Core.ActivityModel.Activity
            {
                Name = "phone activity - " + DateTime.Now,
                Description = "This is the description of the test activity - " + DateTime.Now
            };
            ac.Uri = "http://tempori.org/" + ac.Id;

            ac.Meta.Data = "added meta data";
            ac.Owner = _user;
            return ac;
        }

        #endregion
    }
}

