using CSharpFunctionalExtensions;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.HttpClients;

namespace SOS.Status.Web.Services;

public class ObservationSearchService : IObservationSearchService
{
    private readonly SosObservationsApiClient _observationsApiClient;

    public ObservationSearchService(
        SosObservationsApiClient observationsApiClient)
    {
        _observationsApiClient = observationsApiClient ?? throw new ArgumentNullException(nameof(observationsApiClient));
    }

    public async Task<Client.Dtos.ProcessSummaryDto> GetProcessSummaryAsync()
    {
        var processSummary = await _observationsApiClient.GetProcessSummary();
        return processSummary!;
    }

    public async Task<Client.Dtos.SosObsApi.PagedResultDto<Observation>> SearchObservations(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        var pageResult = await _observationsApiClient.SearchObservationsAsync(
            filter,
            skip,
            take,
            sortBy,
            sortOrder,
            validateSearchFilter,
            translationCultureCode,
            sensitiveObservations);

        return pageResult!;
    }

    public async Task<Result<SearchByCursorResultDto<Observation>>> SearchObservationsByCursor(
        SearchFilterInternalDto filter,
        int take = 1000,
        string? cursor = null,
        string sortBy = "taxon.id",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        var result = await _observationsApiClient.SearchObservationsByCursorAsync(
            filter,
            take,
            cursor,
            sortBy,
            sortOrder,
            validateSearchFilter,
            translationCultureCode,
            sensitiveObservations);

        if (result.IsFailure)
        {
            return Result.Failure<SearchByCursorResultDto<Observation>>(result.Error);
        }

        var dto = result.Value;
        return Result.Success(new SearchByCursorResultDto<Observation>
        {
            NextCursor = dto?.NextCursor,
            Take = dto?.Take ?? 0,
            TotalCount = dto?.TotalCount ?? 0,
            Records = dto?.Records
        });
    }
}