using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NooSphere.Helpers
{
    public static class ObjectToJsonHelper
    {
        public static string Convert(object obj)
        {
            JsonSerializerSettings set = new JsonSerializerSettings();
            set.NullValueHandling = NullValueHandling.Include;
            set.DefaultValueHandling = DefaultValueHandling.Include;
            return JsonConvert.SerializeObject(obj, Formatting.None, set);
        }
    }
}
