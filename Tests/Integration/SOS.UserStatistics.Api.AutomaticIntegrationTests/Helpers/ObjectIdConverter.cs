namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Helpers;

public class ObjectIdConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ObjectId);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
            throw new Exception($"Unexpected token parsing ObjectId. Expected String, got {reader.TokenType}.");

        var value = (string)reader.Value;
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is ObjectId)
        {
            var objectId = (ObjectId)value;
            writer.WriteValue(objectId != ObjectId.Empty ? objectId.ToString() : string.Empty);
        }
        else
        {
            throw new Exception("Expected ObjectId value.");
        }
    }
}