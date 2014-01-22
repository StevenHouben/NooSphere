using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABC.Model;

namespace ABC.Infrastructure.ActivityBase
{
    public class ResourceEventArgs
    {
        public string ActivityId { get; set; }
        public Resource Resource { get; set; }
        public ResourceEventArgs() {}

        public ResourceEventArgs(Resource resource )
        {
            ActivityId = resource.ActivityId;
            Resource = resource;
        }
    }

    public class ResourceRemovedEventArgs
    {
        public string Id { get; set; }
        public ResourceRemovedEventArgs() {}

        public ResourceRemovedEventArgs( string id)
        {
            Id = id;
        }
    }
}
