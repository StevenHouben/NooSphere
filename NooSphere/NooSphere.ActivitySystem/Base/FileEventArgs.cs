/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base
{
    public class FileEventArgs
    {
        public Resource Resource { get; set; }
        public string LocalPath { get; set; }
        public FileEventArgs() { }
        public FileEventArgs(Resource resource)
        {
            Resource = resource;
        }
        public FileEventArgs(Resource resource,string localPath)
        {
            Resource = resource;
            LocalPath = localPath;
        }
    }
    public class GenericEventArgs<T>
    {
        public T Generic { get; set; }
        public GenericEventArgs() { }
        public GenericEventArgs(T generic)
        {
            Generic = generic;
        }
    }
}
