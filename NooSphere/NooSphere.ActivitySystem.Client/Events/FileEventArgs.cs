using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Client.Events
{
    public class FileEventArgs
    {
        public Resource Resource { get; set; }
        public FileEventArgs() { }
        public FileEventArgs(Resource resource)
        {
            this.Resource = resource;
        }
    }
    public class GenericEventArgs<T>
    {
        public T Generic { get; set; }
        public GenericEventArgs() { }
        public GenericEventArgs(T generic)
        {
            this.Generic = generic;
        }
    }
}
