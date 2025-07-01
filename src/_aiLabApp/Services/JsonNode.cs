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
    }
}
