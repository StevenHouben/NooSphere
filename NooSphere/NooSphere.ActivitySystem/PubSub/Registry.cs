using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.ActivityManager;

namespace NooSphere.ActivitySystem.PubSub
{
    public class Registry
    {
        #region Public Methods
        /// <summary>
        /// Initializes  the store with Event enumerator
        /// </summary>
        public static void Initialize()
        {
            foreach (EventType et in Enum.GetValues(typeof(EventType)))
                Register(et);
        }
        /// <summary>
        /// Adds an event type to the registry store
        /// </summary>
        /// <param name="eventType"></param>
        public static void Register(EventType eventType)
        {
            if (!Store.ContainsKey(eventType))
                Store.Add(eventType, new Dictionary<string, object>());
        }
        #endregion

        #region Public Members
        public static Dictionary<EventType, Dictionary<string, object>> Store = new Dictionary<EventType, Dictionary<string, object>>();
        public static Dictionary<string, ConnectedClient> ConnectedClients = new Dictionary<string, ConnectedClient>();
        #endregion
    }
}