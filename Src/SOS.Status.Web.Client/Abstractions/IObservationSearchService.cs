using SOS.Status.Web.Client.Dtos.SosObsApi;

namespace SOS.Status.Web.Client.Abstractions;

public interface IObservationSearchService
{
    Task<PagedResultDto<Observation>> SearchObservations(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false);
}