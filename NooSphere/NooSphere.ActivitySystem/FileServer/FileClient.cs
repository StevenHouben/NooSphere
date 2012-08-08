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
using NooSphere.ActivitySystem.Base;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileClient:IFileStore
    {
        #region Properties
        public string BasePath { get; set; }
        #endregion

        #region Events
        public event FileAddedHandler FileAdded = null;
        public event FileChangedHandler FileChanged = null;
        public event FileRemovedHandler FileRemoved =null;
        #endregion

        #region Private Members

        private readonly Dictionary<Guid, Resource> _files = new Dictionary<Guid, Resource>();

        #endregion
    }

    public class Alias
    {
        public Resource Resource { get; set; }
        public string Path { get; set; }
        public Alias(Resource resource,string path)
        {
            Resource = resource;
            Path = path;
        }
    }
}
