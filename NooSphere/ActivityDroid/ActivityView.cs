using Android.Content;
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

            AddView(_name);
        }

        public string Name
        {
            set { _name.Text = value; }
        }
    }


}