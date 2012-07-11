using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Contracts.NetEvents;

namespace NooSphere.ActivitySystem.ActivityService.ActivityManagement
{
    public class ParticipantStore
    {
        public static Dictionary<Guid, User> Participants = new Dictionary<Guid, User>();

    }
}
