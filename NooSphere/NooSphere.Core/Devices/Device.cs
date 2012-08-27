﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using NooSphere.Core.Primitives;

namespace NooSphere.Core.Devices
{
    public class Device : Noo
    {
        public DeviceType DeviceType { get; set; }
        public DeviceRole DeviceRole { get; set; }
        public DevicePortability DevicePortability { get; set; }
        public int TagValue { get; set; }

        public string Location { get; set; }
        public string BaseAddress { get; set; }
    }
    public enum DeviceType
    {
        Desktop,
        Laptop,
        SmartPhone,
        Tablet,
        Tabletop,
        WallDisplay,
        Custom,
        Unknown
    }
    public enum DevicePortability
    {
        Stationary,
        Mobile
    }
    public enum DeviceRole
    {
        Master,
        Slave,
        Mediator
    }
}
