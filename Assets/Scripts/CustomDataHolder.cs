using Newtonsoft.Json.Linq;

public class CustomDataHolder
{
    public JObject custom_data = new JObject();

    public void AddCustomData<T>(string key, T value)
    {
        custom_data[key] = JToken.FromObject(value);
    }

    public void RemoveCustomData(string key)
    {
        custom_data.Remove(key);
    }

    public JObject GetCustomData(string key)
    {
        return JsonUtils.GetOrDefault(custom_data, key, new JObject());
    }
}