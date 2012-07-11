using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    public class Operation:IEntity
    {
        public string Name { get;set;}

        public string ID { get; set; }

        public string Description { get; set; } 
    }
}
