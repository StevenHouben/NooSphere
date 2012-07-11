using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Contracts.NetEvents;

namespace NooSphere.ActivitySystem.ActivityService.ActivityManagement
{
    public class ActivityStore
    {
        public static Dictionary<Guid, Activity> Activities = new Dictionary<Guid, Activity>();

    }
}
