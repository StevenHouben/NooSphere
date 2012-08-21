using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using NooSphere.Core.ActivityModel;
using NooSphere.Helpers;
using Activity = Android.App.Activity;

namespace ActivityDroid
{
    [Activity(Label = "Activity Cloud", MainLauncher = true, Icon = "@drawable/icon")]
    public class Login : Activity
    {
        #region Properties
        private User _user;
        #endregion

        #region OnCreate
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            var btnGo = FindViewById<Button>(Resource.Id.Go);
            btnGo.Click += BtnGoClick;
        }
        #endregion

        #region Private Methods
        private void SetStatus(string status)
        {
            RunOnUiThread(() => FindViewById<TextView>(Resource.Id.Status).Text = status);
        }

        private void BtnGoClick(object sender, EventArgs e)
        {
            SetStatus("Logging in...");
            ThreadPool.QueueUserWorkItem(o => LogIn());
        }

        private void CreateUser(string baseUrl)
        {
            var email = FindViewById<TextView>(Resource.Id.Email).Text;
            var username = FindViewById<TextView>(Resource.Id.Username).Text;
            var user = new User
            {
                Email = email,
                Name = username
            };
            var added = Rest.Post(baseUrl + "Users", user);
            if (!JsonConvert.DeserializeObject<bool>(added)) return;
            var result = Rest.Get(baseUrl + "Users?email=" + email);
            var u = JsonConvert.DeserializeObject<User>(result);
            this._user = u;
        }
        private void LogIn()
        {
            var baseUrl = Resources.GetString(Resource.String.ApiPath);
            var result = Rest.Get(baseUrl + "Users?email=" + FindViewById<TextView>(Resource.Id.Email).Text);
            var u = JsonConvert.DeserializeObject<User>(result);
            if (u != null) _user = u;
            else CreateUser(baseUrl);
            SetStatus("Succesfully logged in.");
            
            StartMainActivity();
        }

        private void StartMainActivity()
        {
            var intent = new Intent();
            intent.SetClass(this, typeof (Main));
            intent.PutExtra("User", JsonConvert.SerializeObject(_user));
            StartActivity(intent);
        }
        #endregion
    }
}