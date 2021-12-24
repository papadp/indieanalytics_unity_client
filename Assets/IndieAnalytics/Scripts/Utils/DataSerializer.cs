using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace IndieAnalytics
{
    public class DataSerializer<T>
    {
        private string path;
        JsonSerializer serializer = new JsonSerializer();
    
        public DataSerializer(string path)
        {
            this.path = Application.persistentDataPath + "/" + path;
        }

        public void Save(T data)
        {
            using (StreamWriter sw = new StreamWriter(this.path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, data);
            }
        }

        public void Remove()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    
        public T Load()
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
        
            using (StreamReader sr = new StreamReader(this.path))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}

