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
using NooSphere.ActivitySystem.Base;

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
                    var cc = Registry.ConnectedClients[id];
                    var addr = new Uri(string.Format("http://{0}:{1}", cc.Ip, callbackPort)).AbsoluteUri;

                    lock (Concurrency.SubscriberLock)
                        if (!Registry.Store[type].ContainsKey(id))
                            Registry.Store[type].Add(id, addr);
                    res = "succes";
                }
                else
                    res = "Device not registered";
            }
            else
                res = "Device not registered";
            return res;
        }

        /// <summary>
        /// Unsubscribes an object from an event
        /// </summary>
        /// <param name="id">The id of the object that needs to be unsubscribed</param>
        /// <param name="type">The event type</param>
        public void UnSubscribe(string id, EventType type)
        {
            if (!Registry.ConnectedClients.ContainsKey(id)) return;
            if (Registry.Store[type].ContainsKey(id))
            {
                Registry.Store[type].Remove(id);
            }
        }
    }
}
