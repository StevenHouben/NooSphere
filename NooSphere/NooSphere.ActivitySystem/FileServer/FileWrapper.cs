using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileWrapper
    {
        public byte[] Data { get; set; }
        public Resource Resource { get; set; }
    }
}