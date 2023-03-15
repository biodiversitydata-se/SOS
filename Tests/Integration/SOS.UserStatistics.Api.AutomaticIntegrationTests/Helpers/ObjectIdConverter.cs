using System.Text.Json;

namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Helpers;

public class ObjectIdConverter : System.Text.Json.Serialization.JsonConverter<ObjectId>
{
    public override bool CanConvert(Type type)
    {
        return type == typeof(ObjectId);
    }

    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new Exception($"Unexpected token parsing ObjectId. Expected String, got {reader.TokenType}.");
        }
        reader.Read();

        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
    }

    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value != ObjectId.Empty ? value.ToString() : string.Empty);
    }
}