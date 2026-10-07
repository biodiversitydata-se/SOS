using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Search.Input;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SOS.Observations.Api.Helpers;

internal static class SearchFilterCacheKeyHelper
{
    // Shared across controllers and scoped cache services, including the serializer's metadata cache.
    public static JsonSerializerOptions SerializerOptions { get; } = CreateSerializerOptions();
    private static readonly ConditionalWeakTable<JsonSerializerOptions, CacheSerializationOptions> OptionsCache = new();

    public static string Serialize(object filter, JsonSerializerOptions options, bool ignoreBoundingBox = false) =>
        JsonSerializer.Serialize(filter, GetSerializationOptions(filter, options, ignoreBoundingBox));

    public static string CreateKey(string queryString, object? filter, bool ignoreBoundingBox = false)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(queryString ?? string.Empty));
        if (filter != null)
        {
            // Hash UTF-8 directly instead of copying the JSON through strings and a StringBuilder.
            hash.AppendData(JsonSerializer.SerializeToUtf8Bytes(filter,
                GetSerializationOptions(filter, SerializerOptions, ignoreBoundingBox)));
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static JsonSerializerOptions GetSerializationOptions(object filter, JsonSerializerOptions options, bool ignoreBoundingBox)
    {
        if (filter is SearchFilterBaseDto { Geographics: not null } searchFilter &&
            (ignoreBoundingBox || searchFilter.Geographics is GeographicsFilterInputModel))
        {
            var cachedOptions = OptionsCache.GetValue(options, original => new(
                CreateCacheOptions(original, ignoreBoundingBox: false),
                CreateCacheOptions(original, ignoreBoundingBox: true)));
            options = ignoreBoundingBox ? cachedOptions.WithoutBoundingBox : cachedOptions.WithBoundingBox;
        }

        return options;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter(), new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() }
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    private static JsonSerializerOptions CreateCacheOptions(JsonSerializerOptions original, bool ignoreBoundingBox)
    {
        var options = new JsonSerializerOptions(original);
        options.TypeInfoResolver = (original.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(typeInfo =>
        {
            if (typeInfo.Type != typeof(GeographicsFilterDto)) return;

            string PropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
            var geometryName = PropertyName(nameof(GeographicsFilterDto.Geometries));
            for (var index = 0; index < typeInfo.Properties.Count; index++)
            {
                var property = typeInfo.Properties[index];
                if (property.Name == geometryName)
                {
                    // Only the geometry value is replaced; all other DTO properties use normal serialization.
                    // This writes the existing NTS objects directly, without a GeoJSON object or JSON tree copy.
                    var geometries = typeInfo.CreateJsonPropertyInfo(typeof(object), property.Name);
                    geometries.Get = value => value is GeographicsFilterInputModel input
                        ? input.ServerGeometries : ((GeographicsFilterDto)value).Geometries;
                    geometries.Order = property.Order;
                    typeInfo.Properties[index] = geometries;
                }
                else if (ignoreBoundingBox && property.Name == PropertyName(nameof(GeographicsFilterDto.BoundingBox)))
                {
                    property.Get = _ => null;
                }
            }
        });
        options.MakeReadOnly();
        return options;
    }

    private sealed record CacheSerializationOptions(JsonSerializerOptions WithBoundingBox, JsonSerializerOptions WithoutBoundingBox);
}
