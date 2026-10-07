using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>
/// Client for SOS observation endpoints.
/// </summary>
public interface ISosObservationsClient
{
    /// <summary>Gets one observation by occurrence identifier from the public endpoint.</summary>
    Task<Observation?> GetObservationByIdAsync(
        string occurrenceId,
        ObservationByIdOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one observation by occurrence identifier.</summary>
    Task<Observation?> GetObservationByIdInternalAsync(
        string occurrenceId,
        ObservationByIdInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts observations matching a filter.</summary>
    Task<int> CountInternalAsync(
        SearchFilterInternalBaseDto filter,
        CountInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts observations matching a JSON-encoded filter.</summary>
    Task<int> CountInternalJsonAsync(
        string filterJson,
        CountInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches observations and returns a paged geographic result.</summary>
    Task<GeoPagedResultWithMetadataDto<Observation>> ObservationsBySearchInternalAsync(
        SearchFilterInternalDto filter,
        ObservationSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches with a JSON-encoded filter and returns a paged geographic result.</summary>
    Task<GeoPagedResultWithMetadataDto<Observation>> ObservationsBySearchInternalAsync(
        string filterJson,
        ObservationSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches observations using cursor-based pagination.</summary>
    Task<SearchByCursorResultDto<Observation>> ObservationsByCursorAsync(
        SearchFilterDto filter,
        SearchByCursorOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches a JSON-encoded filter using cursor-based pagination.</summary>
    Task<SearchByCursorResultDto<Observation>> ObservationsByCursorJsonAsync(
        string filterJson,
        SearchByCursorOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches observations using the adaptive observation-or-grid strategy.</summary>
    Task<AdaptiveSearchResponse> AdaptiveSearchInternalAsync(
        SearchFilterInternalDto filter,
        AdaptiveSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches a JSON-encoded filter using the adaptive observation-or-grid strategy.</summary>
    Task<AdaptiveSearchResponse> AdaptiveSearchInternalJsonAsync(
        string filterJson,
        AdaptiveSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches observation locations using the adaptive location strategy.</summary>
    Task<AdaptiveSearchLocationsResponse> AdaptiveSearchLocationsInternalAsync(
        SearchFilterInternalDto filter,
        AdaptiveSearchLocationsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches a JSON-encoded filter using the adaptive location strategy.</summary>
    Task<AdaptiveSearchLocationsResponse> AdaptiveSearchLocationsInternalJsonAsync(
        string filterJson,
        AdaptiveSearchLocationsOptions? options = null,
        CancellationToken cancellationToken = default);
}
