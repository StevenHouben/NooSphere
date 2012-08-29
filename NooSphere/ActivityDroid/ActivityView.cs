using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using NooSphere.Core.ActivityModel;

namespace ActivityDroid
{

    public sealed class ActivityView : LinearLayout
    {
        private Activity _activity;
        private TextView _name;

        public ActivityView(Context context, Activity activity)
            : base(context)
        {
            Orientation = Orientation.Vertical;
            _activity = activity;
            _name = new TextView(context) {Text = activity.Name, TextSize = 19};
            SetBackgroundColor(Color.DarkBlue);
            Click += ActivityViewClick;
            LongClick += ActivityViewLongClick;

            AddView(_name, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        }

        private void ActivityViewLongClick(object sender, LongClickEventArgs e)
        {
            ((Main)Context).RemoveActivity(_activity);
            Log.Debug("UI", "Delete activity: " + e);
        }

        static void ActivityViewClick(object sender, System.EventArgs e)
        {
            Log.Debug("UI", "Activity Tabbed: " + e);
        }

        public string Name
        {
            set { _name.Text = value; }
        }
    }


}