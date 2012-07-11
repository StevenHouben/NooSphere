using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    public class Resources:IEntity
    {
        public string Name{ get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public Uri Location { get; set; }
        public FileType Type { get; set; }
    }
    public enum FileType
    {
        Local,
        Cloud
    }
}
