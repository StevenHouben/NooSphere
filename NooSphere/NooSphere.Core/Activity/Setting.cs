using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    /// <summary>
    /// Represents everything from the physical location
    /// to a digital setting.
    /// </summary>
    public class Setting:IEntity
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }
    }
}
