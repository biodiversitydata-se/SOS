using CSharpFunctionalExtensions;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Status.Web.Client.Models;
using SOS.Api.Contracts.Observations.Requests;

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

    Task<LocationMapSearchResult> AdaptiveSearchLocationItemsCachedGeoJson(
        SearchFilterInternalDto filter, LocationMapSearchOptions options, CancellationToken cancellationToken = default);

    Task<Result<SearchByCursorResultDto<Observation>>> SearchObservationsByCursor(
        SearchFilterInternalDto filter,
        int take = 1000,
        string? cursor = null,
        string sortBy = "taxon.id",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false);
}
