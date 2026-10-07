using NetTopologySuite.IO.Converters;
using SOS.Api.Contracts.Locations.Responses;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Processed.Observation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SOS.Observations.Api.Services.Locations;

/// <summary>Writes a LocationDto-shaped response without materializing GeoJSON geometry objects.</summary>
[JsonConverter(typeof(LocationJsonResponseConverter))]
internal sealed class LocationJsonResponse(Location location)
{
    public Location Location { get; } = location;
    public LocationDto Dto { get; } = location.ToLocationDto(includeGeometries: false);
}

internal sealed class LocationJsonResponseConverter : JsonConverter<LocationJsonResponse>
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonTypeInfo<LocationJsonResponse>> ResponseMetadata = new();

    public LocationJsonResponseConverter() { }

    public override LocationJsonResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("This type is only used to write location responses.");

    public override void Write(Utf8JsonWriter writer, LocationJsonResponse value, JsonSerializerOptions options)
    {
        var location = value.Location;
        if (location is null)
        {
            writer.WriteNullValue();
            return;
        }

        // Keep the existing empty-geometry behavior; normal responses use NTS directly.
        if (HasEmptyGeometry(location.Point) || HasEmptyGeometry(location.PointWithBuffer) ||
            HasEmptyGeometry(location.PointWithDisturbanceBuffer))
        {
            JsonSerializer.Serialize(writer, location.ToLocationDto(), options);
            return;
        }

        JsonSerializer.Serialize(writer, value, ResponseMetadata.GetValue(options, CreateResponseMetadata));
    }

    private static JsonTypeInfo<LocationJsonResponse> CreateResponseMetadata(JsonSerializerOptions options)
    {
        var responseOptions = new JsonSerializerOptions(options);
        // Prefer NTS's response writer over the search/index converter, which only supports 2D.
        responseOptions.Converters.Insert(0, new GeoJsonConverterFactory(false,
            GeoJsonConverterFactory.DefaultIdPropertyName, RingOrientationOption.DoNotModify));
        var responseMetadata = JsonMetadataServices.CreateObjectInfo<LocationJsonResponse>(responseOptions,
            new JsonObjectInfoValues<LocationJsonResponse> { PropertyMetadataInitializer = _ => [] });
        var dtoMetadata = options.GetTypeInfo(typeof(LocationDto));
        responseMetadata.NumberHandling = dtoMetadata.NumberHandling;

        // Ordinary fields and their JSON settings come from the DTO, not a second property list.
        foreach (var source in dtoMetadata.Properties)
        {
            var dtoGetter = source.Get;
            if (dtoGetter is null) continue;
            Func<object, object?>? geometryGetter = (source.AttributeProvider as MemberInfo)?.Name switch
            {
                nameof(LocationDto.Point) => static value => ((LocationJsonResponse)value).Location.Point,
                nameof(LocationDto.PointWithBuffer) => static value => ((LocationJsonResponse)value).Location.PointWithBuffer,
                nameof(LocationDto.PointWithDisturbanceBuffer) => static value => ((LocationJsonResponse)value).Location.PointWithDisturbanceBuffer,
                _ => null
            };
            var property = responseMetadata.CreateJsonPropertyInfo(
                geometryGetter is null ? source.PropertyType : typeof(NetTopologySuite.Geometries.Geometry), source.Name);
            property.Get = geometryGetter ?? (value => dtoGetter(((LocationJsonResponse)value).Dto));
            property.AttributeProvider = source.AttributeProvider;
            property.Order = source.Order;
            property.IsGetNullable = source.IsGetNullable;
            property.IsExtensionData = source.IsExtensionData;
            if (geometryGetter is null)
            {
                property.CustomConverter = source.CustomConverter;
                property.NumberHandling = source.NumberHandling;
                if (source.ShouldSerialize is { } shouldSerialize)
                    property.ShouldSerialize = (value, fieldValue) => shouldSerialize(((LocationJsonResponse)value).Dto, fieldValue);
            }
            responseMetadata.Properties.Add(property);
        }
        return responseMetadata;
    }

    private static bool HasEmptyGeometry(NetTopologySuite.Geometries.Geometry? geometry)
    {
        if (geometry is null) return false;
        if (geometry.IsEmpty) return true;
        if (geometry is NetTopologySuite.Geometries.GeometryCollection collection)
        {
            for (var i = 0; i < collection.NumGeometries; i++)
                if (HasEmptyGeometry(collection.GetGeometryN(i))) return true;
        }
        return false;
    }
}
