using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.ActivityService.PubSub
{
    public class Registry
    {
        public static void Register(EventType eventType)
        {
            if (!Store.ContainsKey(eventType))
                Store.Add(eventType, new Dictionary<string, object>());
        }
        public static Dictionary<EventType, Dictionary<string, object>> Store = new Dictionary<EventType, Dictionary<string, object>>();
        public static Dictionary<string, ConnectedClient> ConnectedClients = new Dictionary<string, ConnectedClient>();
    }
}