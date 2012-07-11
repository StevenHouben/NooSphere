using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.Primitives;
using System.IO;

namespace NooSphere.Core.ActivityModel
{
    public class Resource : Identity
    {
        public Resource()
            : base()
        {
        }

        public Guid ActivityId { get; set; }
        public Guid ActionId { get; set; }
        public int Size { get; set; }
        public string CreationTime { get; set; }
        public string LastWriteTime { get; set; }
        public string RelativePath { get; set; }
        public Service Service { get; set; }
    }
}
