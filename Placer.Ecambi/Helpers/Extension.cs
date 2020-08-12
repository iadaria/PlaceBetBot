using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Placer.Ecambi
{
    //All copy
    public static class Extension
    {
        public static Uri UriWithoutWww(this Uri uri)
        {
            string domainWithoutWww = uri.ToString().Replace("www.", "");
            return new Uri(domainWithoutWww);
        }

        public static string GetDumpShort<T>(this T objectToDump)
        {
            foreach(var property in typeof(T).GetProperties())
            {
                if (property.PropertyType.Name == "JToken")
                {
                    property.SetValue(objectToDump, null);
                }
            }
            
            return JsonConvert.SerializeObject(objectToDump, Formatting.Indented);
        }
        
        public static string GetDump<T>(this T objectToDump)
        {
            return JsonConvert.SerializeObject(objectToDump, Formatting.Indented);
        }

        public static string ToCamelCaseJson(this object value)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}
