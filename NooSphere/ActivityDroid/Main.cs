using System;
using System.IO;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Helpers;
using Activity = Android.App.Activity;

namespace ActivityDroid
{
    [Activity(Label = "Main")]
    public class Main : Activity
    {
        #region Properties
        private ActivityClient _client;
        private User _user;
        private Device _device;
        private DiscoveryManager _discovery;
        #endregion

        #region UI Adapter

        private ActivityAdapter _activityAdapter;
        #endregion

        #region OnCreate
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _activityAdapter = new ActivityAdapter(this);
            FindViewById<GridView>(Resource.Id.Activities).Adapter = _activityAdapter;

            RunOnUiThread(() => _activityAdapter.NotifyDataSetChanged());

            var btnAddActivity = FindViewById<Button>(Resource.Id.AddActivity);
            btnAddActivity.Click += BtnAddActivity;

            SetUser(Intent.GetStringExtra("User"));
            SetStatus("Hi " + _user.Name + ", you are logged in.");
            ThreadPool.QueueUserWorkItem(o => StartActivityManager());
        }
        #endregion

        #region Public Methods
        public void AddActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            _client.AddActivity(activity);
        }
        public void RemoveActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            _client.RemoveActivity(activity.Id);
        }
        #endregion

        #region Private Methods
        private void BtnAddActivity(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => AddActivity(GetInitializedActivity()));
        }

        private void SetUser(string json)
        {
            _user = JsonConvert.DeserializeObject<User>(json);
        }

        private void StartActivityManager()
        {
            _discovery = new DiscoveryManager();
            _discovery.DiscoveryAddressAdded += OnDiscoveryAddressAdded;
            _device = new Device
                            {
                                DeviceType = DeviceType.SmartPhone,
                                DevicePortability = DevicePortability.Mobile,
                                Name = Build.Device
                            };
            _discovery.Find();
        }

        private void OnDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            if (_client != null) return;
            var builder = new AlertDialog.Builder(this);
            builder.SetPositiveButton("Yes", (sender, args) =>
            {
                SetStatus("Connecting to " + e.ServiceInfo.Name + "...");
                StartClient(e.ServiceInfo.Address);
            });
            builder.SetNegativeButton("No", (sender, args) => { });
            builder.SetMessage("Found service on " + e.ServiceInfo.Name + ". Do you want to connect?");
            builder.SetTitle("Connect to service");
            RunOnUiThread(() => builder.Show());
        }

        private void StartClient(string activityManagerHttpAddress)
        {
            var path = GetExternalFilesDir("ActivityCloud").AbsolutePath;
            SetStatus("Connecting to Activity Manager on " + activityManagerHttpAddress);
            _client = new ActivityClient(path, _device) { CurrentUser = _user };

            _client.ActivityAdded += OnActivityAdded;
            _client.ActivityChanged += OnActivityChanged;
            _client.ActivityRemoved += OnActivityRemoved;
            _client.MessageReceived += OnMessageReceived;

            _client.FriendAdded += OnFriendAdded;
            _client.FriendDeleted += OnFriendDeleted;
            _client.FriendRequestReceived += OnFriendRequestReceived;

            _client.FileAdded += OnFileAdded;
            _client.FileRemoved += OnFileRemoved;

            _client.ConnectionEstablished += OnConnectionEstablished;

            _client.Open(activityManagerHttpAddress);
        }
        #endregion

        #region UI Changes
        private void AddActivityUI(NooSphere.Core.ActivityModel.Activity activity)
        {
            RunOnUiThread(() => _activityAdapter.Add(activity));
        }
        private void RemoveActivityUI(Guid activityId)
        {
            RunOnUiThread(() => _activityAdapter.Remove(activityId));
        }
        private void SetStatus(string status)
        {
            RunOnUiThread(() => FindViewById<TextView>(Resource.Id.Status).Text = status);
        }
        #endregion

        #region Events

        private void OnActivityAdded(object sender, ActivityEventArgs e)
        {
            Log.Out("Main", "Activity Added");
            AddActivityUI(e.Activity);
        }

        private void OnActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            Log.Out("Main", "Activity Removed");
            RemoveActivityUI(e.Id);
        }

        private void OnActivityChanged(object sender, ActivityEventArgs e)
        {
            Log.Out("Main", "Activity Changed");
        }

        private void OnFileAdded(object sender, FileEventArgs e)
        {
            Log.Out("Main", "File Added");
        }

        private void OnFileRemoved(object sender, FileEventArgs e)
        {
            Log.Out("Main", "File Removed");
        }

        private void OnMessageReceived(object sender, ComEventArgs e)
        {
            Log.Out("Main", "Message Received");
        }

        private void OnFriendAdded(object sender, FriendEventArgs e)
        {
            Log.Out("Main", "Friend Added");
        }

        private void OnFriendDeleted(object sender, FriendDeletedEventArgs e)
        {
            Log.Out("Main", "Friend Deleted");
        }

        private void OnFriendRequestReceived(object sender, FriendEventArgs e)
        {
            Log.Out("Main", "Friend Request Received");
        }

        private void OnConnectionEstablished(object sender, EventArgs e)
        {
            SetStatus("Connection established.");
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

