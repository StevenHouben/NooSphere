using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Base
{
    public class ContextEventArgs
    {
        public string Message { get; set; }
        public ContextEventArgs() { }
        public ContextEventArgs(string message)
        {
           Message = message;
        }
    }
}
