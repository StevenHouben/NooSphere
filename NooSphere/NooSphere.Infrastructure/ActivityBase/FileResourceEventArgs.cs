using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NooSphere.Model;

namespace NooSphere.Infrastructure.ActivityBase
{
    public class FileResourceEventArgs
    {
        public string ActivityId { get; set; }
        public FileResource Resource { get; set; }
        public FileResourceEventArgs() {}

        public FileResourceEventArgs(FileResource resource )
        {
            ActivityId = resource.ActivityId;
            Resource = resource;
        }
    }

    public class FileResourceRemovedEventArgs
    {
        public string Id { get; set; }
        public FileResourceRemovedEventArgs() {}

        public FileResourceRemovedEventArgs( string id)
        {
            Id = id;
        }
    }
}
