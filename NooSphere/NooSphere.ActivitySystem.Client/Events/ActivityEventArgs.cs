using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Contracts.NetEvents;

namespace NooSphere.ActivitySystem.Client.Events
{
    public class ActivityEventArgs
    {
        public Activity Activity { get; set; }
        public ActivityEventArgs() { }
        public ActivityEventArgs(Activity activity)
        {
            this.Activity = activity;
        }
    }
    public class ActivityRemovedEventArgs
    {
        public Guid ID { get; set; }
        public ActivityRemovedEventArgs() { }
        public ActivityRemovedEventArgs(Guid id)
        {
            this.ID = id;
        }
    }
}
