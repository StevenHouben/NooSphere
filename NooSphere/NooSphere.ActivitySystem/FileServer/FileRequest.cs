using System.IO;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileRequest
    {
        public Resource Resouce { get; set; }
        public Stream FileStream { get; set; }
    }
}
