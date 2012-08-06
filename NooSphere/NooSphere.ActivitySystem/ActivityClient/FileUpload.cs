using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.ActivityClient
{
    public class FileUpload
    {
        public byte[] file { get; set; }
        public Resource resource { get; set; }
    }
}
