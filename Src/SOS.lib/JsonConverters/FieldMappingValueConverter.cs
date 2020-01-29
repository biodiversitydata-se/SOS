using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// JSON Converter that can be used to deserialize JSON into different classes
    /// that implements the IFieldMappingValue interface.
    /// </summary>
    public class FieldMappingValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IFieldMappingValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            if (jsonObject.Property("Category") == null)
            {
                var objen = jsonObject.ToObject<FieldMappingValue>();
                return objen;
            }
            else
            {
                var objen = jsonObject.ToObject<FieldMappingWithCategoryValue>();
                return objen;
            }
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}