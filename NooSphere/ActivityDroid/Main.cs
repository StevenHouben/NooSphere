using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using Activity = Android.App.Activity;

namespace ActivityDroid
{
    [Activity(Label = "Main")]
    public class Main : Activity
    {
        #region Private Properties
        private User _user;
        private Device _device;
        private DiscoveryManager _discovery;
        #endregion

        #region
        public static ActivityClient Client;
        #endregion

        #region UI Adapter

        private ActivityAdapter _activityAdapter;
        #endregion

        #region Lifecycle routines
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

            var btnStartRemote = FindViewById<Button>(Resource.Id.StartRemote);
            btnStartRemote.Click += BtnStartRemote;

            var btnAddActivity = FindViewById<Button>(Resource.Id.AddActivity);
            btnAddActivity.Click += BtnAddActivity;

            var btnSendTextMessage = FindViewById<Button>(Resource.Id.SendTextMessage);
            btnSendTextMessage.Click += BtnSendTextMessage;

            SetUser(Intent.GetStringExtra("User"));
            SetStatus("Hi " + _user.Name + ", you are logged in.");
            Task.Factory.StartNew(StartActivityManager);
        }

        protected override void OnDestroy()
        {
            if(Client != null) Client.Close();
            base.OnDestroy();
        }
        #endregion

        #region Public Methods
        public void AddActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            Client.AddActivity(activity);
        }
        public void RemoveActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            Client.RemoveActivity(activity.Id);
        }
        public void ShowMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Long);
        }
        #endregion

        #region Private Methods
        private void SendTextMessage(string number, string message)
        {
            SmsManager.Default.SendTextMessage(number, null, message, null, null);
        }

        private void BtnStartRemote(object sender, EventArgs e)
        {
            var intent = new Intent();
            intent.SetClass(this, typeof(RemoteController));
            intent.PutExtra("ActivityClient", JsonConvert.SerializeObject(Client));
            StartActivity(intent);
        }

        private void BtnSendTextMessage(object sender, EventArgs e)
        {
            var message = FindViewById<EditText>(Resource.Id.TextMessage).Text;
            Task.Factory.StartNew(() => SendTextMessage("+4551842410", message));
        }

        private void BtnAddActivity(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => AddActivity(GetInitializedActivity()));
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
            if (Client != null) return;
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
            Client = new ActivityClient(path, _device) { CurrentUser = _user };

            Client.ActivityAdded += OnActivityAdded;
            Client.ActivityChanged += OnActivityChanged;
            Client.ActivityRemoved += OnActivityRemoved;
            Client.MessageReceived += OnMessageReceived;

            Client.FriendAdded += OnFriendAdded;
            Client.FriendDeleted += OnFriendDeleted;
            Client.FriendRequestReceived += OnFriendRequestReceived;

            Client.FileAdded += OnFileAdded;
            Client.FileRemoved += OnFileRemoved;

            Client.ConnectionEstablished += OnConnectionEstablished;

            Client.Open(activityManagerHttpAddress);
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
            if (e.Message.Type == MessageType.Communication && e.Message.Header == "SendTextMessage")
                SendTextMessage(e.Message.To, e.Message.Content);
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

