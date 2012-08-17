/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base
{
    public class Syncer
    {
        public Dictionary<Guid, Activity> Buffer { get; set; }
        public Dictionary<Guid, ActivityEvent> EventType { get; set; }
        public Dictionary<Guid, List<Resource>> Resources { get; set; }

        public SyncType Type { get; set; }

        public Syncer(SyncType type)
        {
            Type = type;
            Resources = new Dictionary<Guid, List<Resource>>();
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
