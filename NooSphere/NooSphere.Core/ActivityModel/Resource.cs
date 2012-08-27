
﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Globalization;
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ActivityModel
{
    public class Resource : Noo
    {
        public Resource()
        {
            InitializeTimeStamps();
        }
        public Resource(int size,string name)
        {
            InitializeTimeStamps();
            Name = name;
            Size = size;
        }

        private void InitializeTimeStamps()
        {
            CreationTime = DateTime.Now.ToString("u");
            LastWriteTime = DateTime.Now.ToString("u");
        }

        public Guid ActivityId { get; set; }
        public int Size { get; set; }
        public string CreationTime { get; set; }
        public string LastWriteTime { get; set; }
        public string RelativePath { get {return ActivityId +"/"+ Name; }}
        public string CloudPath { get { return "Activities/" + ActivityId + "/Resources/" + Id; } }
    }
}
