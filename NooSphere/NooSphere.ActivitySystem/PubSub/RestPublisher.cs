using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.ServiceModel;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.PubSub
{
    public class RestPublisher
    {
        /// <summary>
        /// Publishes an event to all suscribers
        /// </summary>
        /// <param name="type">The event type</param>
        /// <param name="publishUrl">The url to where the event needs to be published</param>
        /// <param name="netObject">The object that needs to be published</param>
        public void Publish(EventType type,string publishUrl, object netObject)
        {
            Thread t = new Thread(() =>
            {
                List<string> toRemove = new List<string>();
                lock (Concurrency._SubscriberLock)
                {
                    foreach (KeyValuePair<string, object> entry in Registry.Store[type])
                    {
                        try
                        {
                            RestHelper.Post(entry.Value.ToString() + publishUrl, netObject);
                        }
                        catch (FaultException ex)
                        {
                            if (ex == null)
                                toRemove.Add(entry.Key);
                        }
                    }
                    if (toRemove.Count > 0)
                    {
                        foreach (string subscriberAddress in toRemove)
                            Registry.Store[type].Remove(subscriberAddress);
                    }
                }

            });
            t.Start();
        }
    }
}
