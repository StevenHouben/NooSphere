using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ActivityDroid
{

    public class ActivityView : LinearLayout {

        private TextView _name;

        public ActivityView(Context context, NooSphere.Core.ActivityModel.Activity activity)
            : base(context)
        {
            Orientation = Orientation.Vertical;
            _name = new TextView(context) {Text = activity.Name, TextSize = 19};
            SetBackgroundColor(Color.DarkBlue);
            Click += ActivityView_Click;

            AddView(_name, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        }

        void ActivityView_Click(object sender, System.EventArgs e)
        {
            Log.Debug("UI", "Activity Tabbed: " + e);
        }

        public string Name
        {
            set { _name.Text = value; }
        }
    }


}