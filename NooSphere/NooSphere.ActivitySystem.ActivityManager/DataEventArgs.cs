using System;
using Newtonsoft.Json.Linq;

namespace NooSphere.ActivitySystem.ActivityService
{
    public class DataEventArgs : EventArgs
    {
        public JObject Data { get; set; }

        public DataEventArgs(object data)
        {
            Data = (JObject)data;
        }
    }
}
