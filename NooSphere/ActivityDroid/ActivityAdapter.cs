using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
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
            activities = new List<Activity>();
        }

        public void Add(Activity activity)
        {
            lock (activities)
            {
                activities.Add(activity);
                NotifyDataSetChanged();
            }
        }

        public void Remove(Guid activityId)
        {
            lock(activities)
            {
                var act = activities.SingleOrDefault(a => a.Id == activityId);
                if (act != null)
                    activities.Remove(act);
            }
            NotifyDataSetChanged();
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
            return tv;
        }

        public override int Count
        {
            get { return activities.Count; }
        }
    }
}