using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Search.Input;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SOS.Lib.JsonConverters;

/// <summary>
/// Reads HTTP geographic filters directly into NetTopologySuite geometries without creating GeoJSON.Text objects.
/// Registered only by the APIs; clients use the DTO's default JSON serialization.
/// </summary>
public sealed class GeographicsFilterInputJsonConverter : JsonConverter<GeographicsFilterDto>
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> ReadOptions = new();
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> WriteOptions = new();

    public override GeographicsFilterDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("The geographics value must be an object.");
        }

        return JsonSerializer.Deserialize<GeographicsFilterDto>(ref reader,
            ReadOptions.GetValue(options, original => CreateOptions(original, forInput: true)));
    }

    public override void Write(Utf8JsonWriter writer, GeographicsFilterDto value, JsonSerializerOptions options)
    {
        var serializableFilter = value is GeographicsFilterInputModel input ? input.ToSerializableGeographicsFilterDto() : value;
        JsonSerializer.Serialize(writer, serializableFilter,
            WriteOptions.GetValue(options, original => CreateOptions(original, forInput: false)));
    }

    private static JsonSerializerOptions CreateOptions(JsonSerializerOptions original, bool forInput)
    {
        var options = new JsonSerializerOptions(original);
        for (var index = options.Converters.Count - 1; index >= 0; index--)
        {
            if (options.Converters[index] is GeographicsFilterInputJsonConverter)
                options.Converters.RemoveAt(index);
        }

        if (forInput)
        {
            options.TypeInfoResolver = (original.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(typeInfo =>
            {
                if (typeInfo.Type != typeof(GeographicsFilterDto)) return;

                // Bind every ordinary DTO property normally; only the geometry representation is server-specific.
                typeInfo.CreateObject = () => new GeographicsFilterInputModel();
                var property = typeInfo.Properties.Single(property =>
                    property.AttributeProvider is PropertyInfo { Name: nameof(GeographicsFilterDto.Geometries) });
                var geometries = typeInfo.CreateJsonPropertyInfo(typeof(List<Geometry>), property.Name);
                geometries.Set = (filter, value) => ((GeographicsFilterInputModel)filter).ServerGeometries = (List<Geometry>?)value;
                geometries.CustomConverter = new SearchGeometryCollectionConverter();
                geometries.IsRequired = property.IsRequired;
                typeInfo.Properties[typeInfo.Properties.IndexOf(property)] = geometries;
            });
        }

        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    private sealed class SearchGeometryCollectionConverter : JsonConverter<List<Geometry>>
    {
        public override List<Geometry>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var value = document.RootElement;
            if (value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The geometries value must be an array.");
            }

            var geometries = new List<Geometry>(value.GetArrayLength());
            var useDirectReader = options.GetConverter(typeof(Geometry)) is GeometryConverter;
            var index = 0;
            foreach (var element in value.EnumerateArray())
            {
                try
                {
                    if (!GeoJsonGeometryValidator.IsValidStructure(element)) throw InvalidGeometry(index);
                    geometries.Add(useDirectReader
                        ? GeometryConverter.ReadGeometry(element)
                        : element.Deserialize<Geometry>(options) ?? throw InvalidGeometry(index));
                }
                catch (Exception exception) when (exception is JsonException or ArgumentException or NotSupportedException or InvalidOperationException)
                {
                    throw InvalidGeometry(index, exception);
                }
                index++;
            }

            return geometries;
        }

        public override void Write(Utf8JsonWriter writer, List<Geometry> value, JsonSerializerOptions options) =>
            throw new NotSupportedException("Search geometry binding is read-only.");
    }

    private static JsonException InvalidGeometry(int index, Exception? inner = null) =>
        new("Invalid geometry.", $"$.geographics.geometries[{index}]", null, null, inner);
}
