using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    public class Action : IEntity
    {

        public Action()
        {
            this.Operations = new List<Operation>();
        }

        public string Name { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<Operation> Operations { get; set; }
    }
}
