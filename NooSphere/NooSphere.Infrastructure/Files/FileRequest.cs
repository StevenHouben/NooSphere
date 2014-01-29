using System;
using NooSphere.Model;


namespace NooSphere.Infrastructure.Files
{
    public class FileRequest
    {
        public LegacyResource Resource { get; set; }
        public String Bytes { get; set; }
    }
}