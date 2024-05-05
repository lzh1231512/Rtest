using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DL91.Common
{
    public class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static async Task SerializeToStream<T>(T obj, Stream fs)
        {
            await JsonSerializer.SerializeAsync(fs, obj);
        }

        public static T Deserialize<T>(string json)
        { 
            return JsonSerializer.Deserialize<T>(json);
        }

        public static async Task<T> DeserializeFromStream<T>(Stream fs)
        {
            return await JsonSerializer.DeserializeAsync<T>(fs);
        }
    }
}
