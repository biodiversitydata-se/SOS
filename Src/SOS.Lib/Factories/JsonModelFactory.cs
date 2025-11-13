using System.Text.Json.Nodes;

namespace SOS.Lib.Factories;

public static class JsonModelFactory
{
    public static JsonNode CreateFromJson(string json)
        => json is null ? null : JsonNode.Parse(json);
}
