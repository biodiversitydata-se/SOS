using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using System.Text.Json;

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
    public async Task<Observation?> GetObservationByIdWithSensitiveFallbackAsync(
        string occurrenceId,
        ObservationByIdInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var firstAttempt = CloneObservationOptions(options);
        firstAttempt.OutputFieldSet = options?.OutputFieldSet ?? OutputFieldSet.All;
        firstAttempt.ResolveGeneralizedObservations = true;
        firstAttempt.SensitiveObservations = false;

        var observation = await _sosObservationsClient.GetObservationByIdInternalAsync(
            occurrenceId,
            firstAttempt,
            cancellationToken);
        if (observation is not null)
        {
            return observation;
        }

        var sensitiveAttempt = CloneObservationOptions(options);
        sensitiveAttempt.OutputFieldSet = options?.OutputFieldSet ?? OutputFieldSet.All;
        sensitiveAttempt.ResolveGeneralizedObservations = false;
        sensitiveAttempt.SensitiveObservations = true;
        return await _sosObservationsClient.GetObservationByIdInternalAsync(
            occurrenceId,
            sensitiveAttempt,
            cancellationToken);
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
