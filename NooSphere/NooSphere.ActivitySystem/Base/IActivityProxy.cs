using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base
{
    public interface IActivityProxy
    {
        Activity Activity { get; set; }
    }
}
