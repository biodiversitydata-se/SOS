using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>High-level observation search workflows.</summary>
public interface ISosObservationSearchService
{
    /// <summary>
    /// Gets a public observation, and falls back to the sensitive index when absent.
    /// </summary>
    Task<Observation?> GetObservationByIdWithSensitiveFallbackAsync(
        string occurrenceId,
        ObservationByIdInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Determines whether a filter matches at least one sensitive observation.</summary>
    Task<bool> HasSensitiveObservationsAsync(
        SearchFilterInternalBaseDto filter,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}
