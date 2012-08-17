using System;
using System.Collections.Generic;
using System.Drawing;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base
{
    public class Syncer
    {
        public Dictionary<Guid, Point> Counters { get; set; }
        public Dictionary<Guid, Activity> Buffer { get; set; }
        public Dictionary<Guid, ActivityEvent> EventType { get; set; }
        public Dictionary<Guid, Guid> LookUpTable { get; set; }
        public SyncType Type { get; set; }

        public Syncer(SyncType type)
        {
            Type = type;
            Counters = new Dictionary<Guid, Point>();
            Buffer = new Dictionary<Guid, Activity>();
            EventType = new Dictionary<Guid, ActivityEvent>(); 
        }
    }

    public enum SyncType
    {
        Cloud,
        Local
    }
}
