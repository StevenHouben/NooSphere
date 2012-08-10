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
        /// <param name="type">The event type</param>
        /// <param name="publishUrl">The url to where the event needs to be published</param>
        /// <param name="netObject">The object that needs to be published</param>
        /// <param name="source">The source that whishes to publish </param>
        /// <param name="sendToSource">Enables or disable self-publishing to source</param>
        public void Publish(EventType type,string publishUrl, object netObject,object source=null,bool sendToSource=false)
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
                            if (source!= null && entry.Value == source && sendToSource)
                                    Rest.Post(entry.Value + publishUrl, netObject);
                            else Rest.Post(entry.Value + publishUrl, netObject);
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

        /// <summary>
        /// Publish event to one subscriber
        /// </summary>
        /// <param name="type"></param>
        /// <param name="publishUrl"></param>
        /// <param name="netObject"></param>
        /// <param name="subscriber"> </param>
        public void PublishToSubscriber(EventType type, string publishUrl, object netObject,object subscriber)
        {
            var t = new Thread(() =>
            {
                lock (Concurrency.SubscriberLock)
                {
                        try
                        {
                            Rest.Post(subscriber + publishUrl, netObject);
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                }

            });
            t.Start();
        }
    }
}
