using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.ActivitySystem.Contracts.NetEvents;

namespace NooSphere.ActivitySystem.Events
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
