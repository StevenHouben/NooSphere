using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Activity = NooSphere.Core.ActivityModel.Activity;
using Object = Java.Lang.Object;

namespace ActivityDroid
{
    public class ActivityAdapter : BaseAdapter {

        private Context context;
        private List<Activity> activities;

        public ActivityAdapter(Context context)
        {
            this.context = context;
            activities = new List<Activity> {GetInitializedActivity()};
        }
        private Activity GetInitializedActivity()
        {
            var ac = new Activity
            {
                Name = "phone activity - " + DateTime.Now,
                Description = "This is the description of the test activity - " + DateTime.Now
            };
            ac.Uri = "http://tempori.org/" + ac.Id;

            ac.Meta.Data = "added meta data";
            return ac;
        }

        public void Add(Activity activity)
        {
            activities.Add(activity);
            NotifyDataSetChanged();
            NotifyDataSetInvalidated();
        }

        public override Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position) {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent) {
            ActivityView tv;
            if (convertView == null)
            {
                tv = new ActivityView(context, activities.ElementAt(position));
            }
            else {
                tv = (ActivityView)convertView;
            }
            tv.Name = activities[position].Name;
            var rnd = new Random(); 
            tv.SetBackgroundColor(Color.Argb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256)));
            return tv;
        }

        public override int Count
        {
            get { return activities.Count; }
        }
    }
}