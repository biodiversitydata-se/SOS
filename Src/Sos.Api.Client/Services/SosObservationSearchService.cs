using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using System.Text.Json;
using GeoJSON.Text.Feature;

namespace SOS.Api.Client.Services;

/// <inheritdoc />
public sealed class SosObservationSearchService : ISosObservationSearchService
{
    private readonly ISosObservationsClient _sosObservationsClient;

    /// <summary>Creates the service using the raw observation client.</summary>
    public SosObservationSearchService(ISosObservationsClient sosObservationsClient)
    {
        _sosObservationsClient = sosObservationsClient ?? throw new ArgumentNullException(nameof(sosObservationsClient));
    }

    /// <inheritdoc />
    public async Task<bool> HasSensitiveObservationsAsync(
        SearchFilterInternalBaseDto filter,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var filterJson = JsonSerializer.SerializeToNode(
            filter,
            filter.GetType(),
            SosClientHttp.SerializerOptions)!.AsObject();
        filterJson["protectionFilter"] = ProtectionFilterDto.Sensitive.ToString();

        var count = await _sosObservationsClient.CountInternalJsonAsync(
            filterJson.ToJsonString(SosClientHttp.SerializerOptions),
            new CountInternalOptions
            {
                RoleId = options?.RoleId,
                AuthorizationApplicationIdentifier = options?.AuthorizationApplicationIdentifier,
                SensitiveObservations = true
            },
            cancellationToken);

        return count > 0;
    }

    /// <inheritdoc />
    public async Task<(AdaptiveSearchLocationsResponse Response, FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto> FeatureCollection)> AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
        SearchFilterInternalDto filter,
        AdaptiveSearchLocationsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var response = await _sosObservationsClient.AdaptiveSearchLocationsInternalAsync(
            filter,
            options,
            cancellationToken);
        var featureCollection = ParseAdaptiveSearchLocationsGeoJson(response.GeoJson);
        return (response, featureCollection);
    }

    private static FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto> ParseAdaptiveSearchLocationsGeoJson(string? geoJson)
    {
        if (string.IsNullOrWhiteSpace(geoJson))
        {
            return new FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto>();
        }

        var featureCollection = JsonSerializer.Deserialize<FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto>>(
            geoJson, SosClientHttp.SerializerOptions)
            ?? new FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto>();

        for (var index = 0; index < featureCollection.Features.Count; index++)
        {
            var feature = featureCollection.Features[index];
            if (feature.Properties is not null) continue;

            // Properties is init-only; replace the feature to supply defaults while retaining its GeoJSON data.
            featureCollection.Features[index] = new()
            {
                Id = feature.Id,
                Geometry = feature.Geometry,
                Properties = new AdaptiveSearchLocationFeaturePropertiesDto(),
                BoundingBoxes = feature.BoundingBoxes,
                CRS = feature.CRS
            };
        }

        return featureCollection;
    }

    private static ObservationByIdInternalOptions CloneObservationOptions(
        ObservationByIdInternalOptions? source)
    {
        return new ObservationByIdInternalOptions
        {
            RoleId = source?.RoleId,
            AuthorizationApplicationIdentifier = source?.AuthorizationApplicationIdentifier,
            OccurrenceId = source?.OccurrenceId,
            OutputFieldSet = source?.OutputFieldSet ?? OutputFieldSet.Minimum,
            TranslationCultureCode = source?.TranslationCultureCode ?? "sv-SE"
        };
    }

}
