using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core.Primitives
{
    public class Identity
    {
        public Identity()
        {
            this.Name = "default";
            this.Id = Guid.NewGuid();
            this.Description = "default activity";
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Uri { get; set; }
        public bool Equals(Identity id)
        {
            return this.Id == id.Id;
        }
    }
}
