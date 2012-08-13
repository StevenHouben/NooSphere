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
        /// <param name="publishUrl"> </param>
        /// <param name="netObject">The object that needs to be published</param>
        /// <param name="source">The source that whishes to publish </param>
        /// <param name="sendToSource">Enables or disable self-publishing to source</param>
        public void Publish(string publishUrl, object netObject, object source = null, bool sendToSource = false)
        {
            Log.Out("Publisher", string.Format("Published {0}",publishUrl), LogCode.Net);
            var toRemove = new List<string>();
            var t = new Thread(() =>
            {
                lock (Concurrency.SubscriberLock)
                {
                    foreach (var entry in Registry.ConnectedClients)
                    {
                        try
                        {
                            if (source != null && entry.Value == source && sendToSource)
                                Rest.Post(entry.Value.Device.BaseAddress + publishUrl, netObject);
                            else Rest.Post(entry.Value.Device.BaseAddress + publishUrl, netObject);
                        }
                        catch (Exception)
                        {
                            
                            toRemove.Add(entry.Key);
                        }

                    }
                    if (toRemove.Count > 0)
                    {
                        foreach (var id in toRemove)
                            Registry.ConnectedClients.Remove(id);
                    }
                }

            });
            t.Start();
        }

        /// <summary>
        /// Publish event to one subscriber
        /// </summary>
        /// <param name="publishUrl"></param>
        /// <param name="netObject"></param>
        /// <param name="subscriber"> </param>
        public void PublishToSubscriber(string publishUrl, object netObject,object subscriber)
        {
            Log.Out("Publisher", string.Format("Publishing {0} to {1}", publishUrl,subscriber), LogCode.Net);
            var t = new Thread(() =>
            {
                lock (Concurrency.SubscriberLock)
                {
                    Rest.Post(subscriber + publishUrl, netObject);
                }

            });
            t.Start();
        }
    }
}
