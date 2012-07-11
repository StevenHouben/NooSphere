using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core
{
    public class Actor:IEntity
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public void Login(){}
        public void Logout(){}
    }
}
