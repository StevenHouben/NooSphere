using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
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
        private Host _host;
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
            _host = new Host(_user, new ContextWrapper(this).GetDir("ActivityCloud", FileCreationMode.Private));
        }

        private void AddActivity(NooSphere.Core.ActivityModel.Activity activity)
        {
            _host.ActivityManager.AddActivity(activity, _device.Id.ToString());
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

