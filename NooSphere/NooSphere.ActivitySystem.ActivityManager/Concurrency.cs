using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.ActivityService
{
    public class Concurrency
    {
        public static object _SubscriberLock = new object();
    }
}
