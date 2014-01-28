using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NooSphere.Infrastructure.Web;

namespace NooSphere.Infrastructure.ActivityBase
{
    public class MessageEventArgs
    {
        public NooMessage Message { get; set; }
        public MessageEventArgs() {}

        public MessageEventArgs(NooMessage message)
        {
            Message = message;
        }
    }
}
