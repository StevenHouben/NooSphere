using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;

namespace ActivityDroid
{
    [Activity(Label = "RemoteController")]
    public class RemoteController : Activity
    {
        private ActivityClient _client;
        private float initialX = 0;
        private float initialY = 0;
        private float deltaX = 0;
        private float deltaY = 0;  

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            _client = JsonConvert.DeserializeObject<ActivityClient>(Intent.GetStringExtra("ActivityClient"));

            SetContentView(Resource.Layout.RemoteController);
            
            var layout = FindViewById<LinearLayout>(Resource.Id.RemoteController);
            layout.Touch += OnTouch;
        }

        void OnTouch(object sender, View.TouchEventArgs e)
        {
            if(e.Event.Action == MotionEventActions.Move)
            {
                var message = new NooSphere.ActivitySystem.Base.Message {Type = MessageType.Control};

                float deltaX, deltaY;
                try
                {
                    deltaX = e.Event.GetX() - initialX;
                    deltaY = e.Event.GetY() - initialY;
                    initialX = e.Event.GetX();
                    initialY = e.Event.GetY();
                } catch(Exception)
                {
                    deltaX = 0;
                    deltaY = 0;
                }
                Log.Debug("INFO", deltaX + "x" + deltaY + " (" + e.Event.PointerCount + ")");
                Main.Client.SendMessage(message);
            }
        }
    }
}