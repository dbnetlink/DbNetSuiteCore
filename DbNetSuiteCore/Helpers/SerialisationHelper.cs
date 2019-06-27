using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace DbNetSuiteCore.Helpers
{
    public static class SerialisationHelper
    {
        public static string SerialiseToJson(object obj)
        {
            return JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }

        public static T DeserialiseJson<T>(Stream stream)
        {
            string json = string.Empty;
            using (StreamReader inputStream = new StreamReader(stream))
            {
                json = inputStream.ReadToEnd();
            }

            return DeserialiseJson<T>(json);
        }

        public static T DeserialiseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
