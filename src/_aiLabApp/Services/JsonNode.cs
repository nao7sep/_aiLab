using System.Text.Json;
using System.Text;

namespace _aiLabApp.Services
{
    public class JsonNode
    {
        private readonly JsonElement _element;

        private JsonNode(JsonElement element)
        {
            _element = element;
        }

        public static JsonNode LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            return new JsonNode(element);
        }

        public JsonNode? Get(string key)
        {
            if (_element.ValueKind == JsonValueKind.Object && _element.TryGetProperty(key, out var value))
            {
                return new JsonNode(value);
            }
            return null;
        }

        public JsonNode? GetByPath(string path)
        {
            var keys = path.Split('/');
            JsonElement current = _element;
            foreach (var key in keys)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(key, out var next))
                {
                    current = next;
                }
                else
                {
                    return null;
                }
            }
            return new JsonNode(current);
        }

        public string? AsString() => _element.ValueKind == JsonValueKind.String ? _element.GetString() : null;
        public int? AsInt() => _element.ValueKind == JsonValueKind.Number && _element.TryGetInt32(out var v) ? v : null;
        public double? AsDouble() => _element.ValueKind == JsonValueKind.Number && _element.TryGetDouble(out var v) ? v : null;
        public bool? AsBool() => _element.ValueKind == JsonValueKind.True || _element.ValueKind == JsonValueKind.False ? _element.GetBoolean() : null;
        public IEnumerable<JsonNode>? AsArray() => _element.ValueKind == JsonValueKind.Array ? _element.EnumerateArray().Select(e => new JsonNode(e)) : null;
        public bool IsNull() => _element.ValueKind == JsonValueKind.Null;
        public bool Exists() => _element.ValueKind != JsonValueKind.Undefined;

        /// <summary>
        /// Returns all direct child key-value pairs as a dictionary, preserving primitive types where possible.
        /// </summary>
        public Dictionary<string, object?> GetChildrenAsDictionary()
        {
            if (_element.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("JsonNode does not contain an object with children.");
            var dict = new Dictionary<string, object?>();
            foreach (var prop in _element.EnumerateObject())
            {
                switch (prop.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        dict[prop.Name] = prop.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        if (prop.Value.TryGetInt32(out int intVal))
                            dict[prop.Name] = intVal;
                        else if (prop.Value.TryGetDouble(out double doubleVal))
                            dict[prop.Name] = doubleVal;
                        else
                            throw new InvalidOperationException($"Number value for '{prop.Name}' could not be converted to int or double.");
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        dict[prop.Name] = prop.Value.GetBoolean();
                        break;
                    case JsonValueKind.Null:
                        dict[prop.Name] = null;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported or unknown value kind '{prop.Value.ValueKind}' for property '{prop.Name}'.");
                }
            }
            return dict;
        }
    }
}
