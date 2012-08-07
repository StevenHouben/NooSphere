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
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Base;

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