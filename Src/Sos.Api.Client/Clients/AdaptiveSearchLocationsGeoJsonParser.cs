using System.Text.Json;
using GeoJSON.Text.Feature;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Client.Clients;

/// <summary>
/// Parses adaptive search locations GeoJSON responses into strongly-typed feature collections.
/// </summary>
internal static class AdaptiveSearchLocationsGeoJsonParser
{
    public static AdaptiveSearchLocationsFeatureCollectionDto Parse(string? geoJson)
    {
        if (string.IsNullOrWhiteSpace(geoJson))
        {
            return new AdaptiveSearchLocationsFeatureCollectionDto();
        }

        var rawFeatureCollection = JsonSerializer.Deserialize<FeatureCollection>(geoJson, SosClientHttp.SerializerOptions)
            ?? new FeatureCollection();

        var features = new List<AdaptiveSearchLocationsFeatureDto>(rawFeatureCollection.Features.Count);
        foreach (var feature in rawFeatureCollection.Features)
        {
            var propertiesElement = JsonSerializer.SerializeToElement(feature.Properties, SosClientHttp.SerializerOptions);
            var properties = propertiesElement.Deserialize<AdaptiveSearchLocationFeaturePropertiesDto>(SosClientHttp.SerializerOptions)
                ?? new AdaptiveSearchLocationFeaturePropertiesDto();

            features.Add(new AdaptiveSearchLocationsFeatureDto
            {
                Id = feature.Id,
                Geometry = feature.Geometry,
                Properties = properties
            });
        }

        return new AdaptiveSearchLocationsFeatureCollectionDto
        {
            Features = features,
            BoundingBox = rawFeatureCollection.BoundingBoxes
        };
    }
}

