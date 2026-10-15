using GeoJSON.Text.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SOS.Lib.JsonConverters;

/// <summary>
/// Reads the position objects stored in Hangfire arguments by Newtonsoft.Json.
/// GeoJSON.Text only provides converters for System.Text.Json.
/// </summary>
public sealed class GeoJsonTextPositionConverter : JsonConverter
{
    // Leave serialization unchanged so new and already stored jobs use the same object format.
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) =>
        objectType == typeof(Position) || objectType == typeof(IPosition);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var position = JObject.Load(reader);
        var latitude = ReadCoordinate(position, nameof(Position.Latitude), required: true)!.Value;
        var longitude = ReadCoordinate(position, nameof(Position.Longitude), required: true)!.Value;
        var altitude = ReadCoordinate(position, nameof(Position.Altitude), required: false);

        return new Position(latitude, longitude, altitude);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotSupportedException("This converter only reads stored positions.");

    private static double? ReadCoordinate(JObject position, string propertyName, bool required)
    {
        var value = position.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
        if (value is null || value.Type == JTokenType.Null)
        {
            if (required)
            {
                throw new JsonSerializationException($"A stored position requires '{propertyName}'.");
            }

            return null;
        }

        if (value.Type is not (JTokenType.Integer or JTokenType.Float))
        {
            throw new JsonSerializationException($"A stored position's '{propertyName}' must be a number.");
        }

        return value.Value<double>();
    }
}
