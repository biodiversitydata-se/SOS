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
}