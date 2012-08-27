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
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.Core.ActivityModel;
using System.Collections.Concurrent;

namespace NooSphere.ActivitySystem.Base.Service
{
    public class Syncer
    {
        public ConcurrentDictionary<Guid, Activity> Buffer { get; set; }
        public ConcurrentDictionary<Guid, ActivityEvent> EventType { get; set; }
        public ConcurrentDictionary<Guid, List<Resource>> Resources { get; set; }

        public SyncType Type { get; set; }

        public Syncer(SyncType type)
        {
            Type = type;
            Resources = new ConcurrentDictionary<Guid, List<Resource>>();
            Buffer = new ConcurrentDictionary<Guid, Activity>();
            EventType = new ConcurrentDictionary<Guid, ActivityEvent>(); 
        }
    }

    public enum SyncType
    {
        Cloud,
        Local
    }
}
