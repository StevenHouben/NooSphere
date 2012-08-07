using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem
{
    public interface IActivityNode
    {
        event InitializedHandler Initialized;
        event ConnectionEstablishedHandler ConnectionEstablished;
    }
}
