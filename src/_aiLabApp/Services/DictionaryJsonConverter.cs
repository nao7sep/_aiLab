using System.Text.Json.Nodes;

namespace _aiLabApp.Services
{
    /// <summary>
    /// Utility for converting a Dictionary<string, object?> to a JsonObject, recursively handling nested dictionaries and lists.
    /// </summary>
    public static class DictionaryJsonConverter
    {
        public static JsonObject ToJsonObject(Dictionary<string, object?> dict)
        {
            var json = new JsonObject();
            foreach (var kvp in dict)
                json[kvp.Key] = ToJsonNode(kvp.Value);
            return json;
        }

        public static JsonNode? ToJsonNode(object? value)
        {
            if (value == null)
                return null;
            if (value is Dictionary<string, object?> dict)
                return ToJsonObject(dict);
            if (value is List<object?> list)
            {
                var array = new JsonArray();
                foreach (var item in list)
                    array.Add(ToJsonNode(item));
                return array;
            }
            // For primitive types and other serializable objects
            return JsonValue.Create(value);
        }
    }
}
