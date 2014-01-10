using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ABC.Infrastructure.Helpers
{
    public class Json
    {
        public static T ConvertFromTypedJson<T>( string json )
        {
            return (T)JsonConvert.DeserializeObject( json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects } );
        }

        public static string ConvertToTypedJson( object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects } );
        }

        public static List<T> ConvertArrayFromTypedJson<T>( string json )
        {
            var array = JArray.Parse( json );
            var list = array.Select( item => ConvertFromTypedJson<T>( item.ToString() ) ).ToList();
            //array.ToObject<List<SelectableEnumItem>>()
            return list;
        }
    }
}