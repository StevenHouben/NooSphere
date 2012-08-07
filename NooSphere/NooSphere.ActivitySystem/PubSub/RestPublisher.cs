/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Collections.Generic;
using System.Threading;
using NooSphere.ActivitySystem.Contracts;
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
            var t = new Thread(() =>
            {
                var toRemove = new List<string>();
                lock (Concurrency.SubscriberLock)
                {
                    foreach (var entry in Registry.Store[type])
                    {
                        try
                        {
                            Rest.Post(entry.Value + publishUrl, netObject);
                        }
                        catch
                        {
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
