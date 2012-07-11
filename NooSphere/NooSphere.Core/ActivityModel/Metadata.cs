using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core.ActivityModel
{
    public class Metadata
    {
        public Metadata()
        {
            this.Data = new object();
        }
        public string Header { get; set; }
        public object Data { get; set; }
    }
}
