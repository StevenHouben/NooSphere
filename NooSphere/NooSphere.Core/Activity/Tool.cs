using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    /// <summary>
    /// Represents everything from an application to
    /// a physical tool.
    /// </summary>
    public class Tool:IEntity
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public ToolType Type { get; set; }
    }
    public enum ToolType
    {
        Physical,
        Digital,
        Mediating
    }
}
