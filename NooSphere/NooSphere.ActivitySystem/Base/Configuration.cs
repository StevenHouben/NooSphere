using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Base
{
    public enum Configuration
    {
        Client,             //Run the client only
        Host,               //Run the host only
        ClientAndHost,      //Run both client and host
        ClientAndManager    //Run both client and local manager
    }
}
