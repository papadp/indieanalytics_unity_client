using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IndieAnalyticsInternal
{
    public static class JsonUtils
    {
        public static T GetOrDefault<T>(JObject json, string selector, T default_value)
        {
            try
            {
                return json.SelectToken(selector).ToObject<T>();
            }
            catch (Exception e)
            {
                Debug.LogWarning(String.Format("Cant find key {0} in json, exception {1}", selector, e.ToString()));
                return default_value;
            }
        }
    }
}
