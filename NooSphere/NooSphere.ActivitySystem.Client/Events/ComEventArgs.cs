using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Client.Events
{
    public class ComEventArgs
    {
        public string Message { get; set; }
        public ComEventArgs() { }
        public ComEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
