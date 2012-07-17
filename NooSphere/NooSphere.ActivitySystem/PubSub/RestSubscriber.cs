using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using System.ServiceModel;
using NooSphere.ActivitySystem.ActivityManager;

namespace NooSphere.ActivitySystem.PubSub
{
    public class RestSubscriber
    {
        /// <summary>
        /// Subscribes a request to an event
        /// </summary>
        /// <param name="id">The id of the requesting object</param>
        /// <param name="type">The event type</param>
        /// <param name="callbackPort">The callback port</param>
        /// <returns></returns>
        public string Subscribe(string id, EventType type, int callbackPort)
        {
            string res;
            if (Registry.ConnectedClients.ContainsKey(id))
            {
                if (callbackPort != -1)
                {
                    ConnectedClient cc = Registry.ConnectedClients[id];
                    string addr = new Uri(string.Format("http://{0}:{1}", cc.IP, callbackPort)).AbsoluteUri;

                    lock (Concurrency._SubscriberLock)
                        if (!Registry.Store[type].ContainsKey(id))
                            Registry.Store[type].Add(id, addr);
                    res = "succes";
                }
                else
                    res = "Device not registered";
            }
            else
                res= "Device not registered";
            return res;
        }

        /// <summary>
        /// Unsubscribes an object from an event
        /// </summary>
        /// <param name="id">The id of the object that needs to be unsubscribed</param>
        /// <param name="type">The event type</param>
        public void UnSubscribe(string id, EventType type)
        {
            if (Registry.ConnectedClients.ContainsKey(id))
            {
                if (Registry.Store[type].ContainsKey(id))
                {
                    object subscriber = Registry.Store[type][id];
                    ConnectedClient cc = Registry.ConnectedClients[id];
                    Registry.Store[type].Remove(id);
                }
            }
        }
    }
}
