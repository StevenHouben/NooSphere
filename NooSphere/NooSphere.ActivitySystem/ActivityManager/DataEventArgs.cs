/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using Newtonsoft.Json.Linq;

namespace NooSphere.ActivitySystem.ActivityManager
{
    public class DataEventArgs : EventArgs
    {
        public JObject Data { get; set; }

        public DataEventArgs(object data)
        {
            Data = (JObject)data;
        }
    }
}
