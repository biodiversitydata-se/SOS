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

    Task<(string GeoJson, long? ObservationsTotalCount, long? EstimatedLocationCount, long? FeatureCount, long? LocationsBeforeClustering, long ResponseSizeBytes, string? CacheStatus, string? ResultType, string? Timings)> AdaptiveSearchLocationItemsCachedGeoJson(
        SearchFilterInternalDto filter,
        int observationsLimit = 10000,
        int maxGridCells = 10000,
        int? locationAggregationLimit = null,
        AdaptiveSearchLocationsStrategy adaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.ObservationsThenGrid,
        bool cluster = true,
        string clusteringAlgorithm = "Greedy",
        CreatePolygonsMode createPolygons = CreatePolygonsMode.No,
        double clusterRadiusPx = 40.0,
        int minLocationsPerCluster = 2,
        int minLocationCountBeforeClustering = 100,
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false,
        double? metersPerPixel = null,
        string? bbox = null,
        bool createSampleObservations = false,
        bool useCache = true,
        int? cutoffYear = null,
        bool includeFilterAreasInResult = false);

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
