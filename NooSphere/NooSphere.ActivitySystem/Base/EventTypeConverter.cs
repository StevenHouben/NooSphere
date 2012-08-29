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
using NooSphere.ActivitySystem.Base.Client;

namespace NooSphere.ActivitySystem.Base
{
    public static class EventTypeConverter
    {
        /// <summary>
        /// Converts an enumeration into a service type
        /// </summary>
        /// <param name="type">The EventType enumerator</param>
        /// <returns>The type that is represented by the emum</returns>
        public static Type TypeFromEnum(EventType type)
        {
            switch (type)
            {
                case EventType.ActivityEvents:
                    return typeof(IActivityNetEvent);
                case EventType.ComEvents:
                    return typeof(IComNetEvent);
                case EventType.DeviceEvents:
                    return typeof(IDeviceNetEvent);
                case EventType.FileEvents:
                    return typeof(IFileNetEvent);
                case EventType.UserEvent:
                    return typeof(IUserEvent);
                default:
                    return null;
            }
        }
    }
}
