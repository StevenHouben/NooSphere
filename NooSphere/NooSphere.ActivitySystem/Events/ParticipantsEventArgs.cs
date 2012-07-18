using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Events
{
    public class ParticipantEventArgs
    {
        public User Participant { get; set; }
        public Guid ActivityId{get;set;}
        public ParticipantEventArgs() { }
        public ParticipantEventArgs(User participant,Guid activityId)
        {
            this.Participant = participant;
            this.ActivityId = activityId;
        }
    }
}
