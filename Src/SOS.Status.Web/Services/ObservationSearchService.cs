using ClientRequests = SOS.Api.Client.Requests;
using ApiModels = SOS.Api.Contracts.Observations.Requests;
using ContractSearchSortOrder = SOS.Api.Contracts.Observations.Requests.SearchSortOrder;
using OutputFilterDto = SOS.Api.Contracts.Observations.Requests.OutputFilterDto;
using SearchFilterDto = SOS.Api.Contracts.Observations.Requests.SearchFilterDto;
using SearchFilterInternalDto = SOS.Api.Contracts.Observations.Requests.SearchFilterInternalDto;
using CSharpFunctionalExtensions;
using SOS.Api.Client.Abstractions;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Models;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Status.Web.Services;

public class ObservationSearchService : IObservationSearchService
{
    private readonly ISosObservationsClient _observationsClient;

    public ObservationSearchService(ISosObservationsClient observationsClient)
    {
        _observationsClient = observationsClient ?? throw new ArgumentNullException(nameof(observationsClient));
    }

    public async Task<PagedResultDto<Observation>> SearchObservations(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        var pageResult = await _observationsClient.ObservationsBySearchInternalAsync(
            filter,
            new ClientRequests.ObservationSearchOptions
            {
                Skip = skip,
                Take = take,
                SortBy = sortBy,
                SortOrder = ParseSearchSortOrder(sortOrder),
                ValidateSearchFilter = validateSearchFilter,
                TranslationCultureCode = translationCultureCode,
                SensitiveObservations = sensitiveObservations
            });

        return new PagedResultDto<Observation>
        {
            Skip = pageResult.Skip,
            Take = pageResult.Take,
            TotalCount = pageResult.TotalCount,
            Records = pageResult.Records.ToList()
        };
    }

    public async Task<(string GeoJson, long? ObservationsTotalCount, long? EstimatedLocationCount, long? FeatureCount, long? LocationsBeforeClustering, long ResponseSizeBytes, string? CacheStatus, string? ResultType, string? Timings)> AdaptiveSearchLocationItemsCachedGeoJson(
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
        bool includeFilterAreasInResult = false)
    {
        var response = await _observationsClient.AdaptiveSearchLocationsInternalAsync(
            filter,
            new ClientRequests.AdaptiveSearchLocationsOptions
            {
                ObservationsLimit = observationsLimit,
                MaxGridCells = maxGridCells,
                SensitiveObservations = sensitiveObservations,
                Cluster = cluster,
                ClusteringAlgorithm = ParseClusteringAlgorithm(clusteringAlgorithm),
                MetersPerPixel = metersPerPixel,
                ClusterRadiusPx = clusterRadiusPx,
                MinLocationsPerCluster = minLocationsPerCluster,
                MinLocationCountBeforeClustering = minLocationCountBeforeClustering,
                CreatePolygons = (ApiModels.CreatePolygonsMode)createPolygons,
                BoundingBoxQuery = bbox,
                IncludeFilterAreasInResult = includeFilterAreasInResult,
                UseCache = useCache,
                CreateSampleObservations = createSampleObservations,
                LocationAggregationLimit = locationAggregationLimit,
                AdaptiveSearchStrategy = (ApiModels.AdaptiveSearchLocationsStrategy)adaptiveSearchStrategy,
                CutoffYear = cutoffYear
            });

        return (
            response.GeoJson,
            response.ObservationsTotalCount,
            response.EstimatedLocationCount,
            response.FeatureCount,
            response.LocationsBeforeClustering,
            response.ResponseSizeBytes,
            response.CacheStatus,
            response.ResultType,
            response.Timings);
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
        var response = await _observationsClient.ObservationsByCursorAsync(
            ToSearchFilterDto(filter),
            new ClientRequests.SearchByCursorOptions
            {
                Take = take,
                Cursor = cursor,
                SortBy = sortBy,
                SortOrder = ParseSearchSortOrder(sortOrder),
                ValidateSearchFilter = validateSearchFilter,
                TranslationCultureCode = translationCultureCode,
                SensitiveObservations = sensitiveObservations
            });

        return Result.Success(new SearchByCursorResultDto<Observation>
        {
            NextCursor = response.NextCursor,
            Take = response.Take,
            TotalCount = response.TotalCount,
            Records = response.Records.ToList()
        });
    }

    private static ApiModels.LocationClusteringAlgorithm ParseClusteringAlgorithm(string clusteringAlgorithm)
    {
        if (string.Equals(clusteringAlgorithm, "Greedy", StringComparison.OrdinalIgnoreCase))
        {
            return ApiModels.LocationClusteringAlgorithm.Sweref99TmGreedy;
        }

        if (string.Equals(clusteringAlgorithm, "FastGreedy", StringComparison.OrdinalIgnoreCase))
        {
            return ApiModels.LocationClusteringAlgorithm.Sweref99TmFastGreedy;
        }

        return Enum.TryParse<ApiModels.LocationClusteringAlgorithm>(clusteringAlgorithm, ignoreCase: true, out var value)
            ? value
            : ApiModels.LocationClusteringAlgorithm.Sweref99TmFastGreedy;
    }

    private static ContractSearchSortOrder ParseSearchSortOrder(string sortOrder)
    {
        return Enum.TryParse<ContractSearchSortOrder>(sortOrder, ignoreCase: true, out var value)
            ? value
            : ContractSearchSortOrder.Asc;
    }

    private static SearchFilterDto ToSearchFilterDto(SearchFilterInternalDto filter)
    {
        return new SearchFilterDto
        {
            BirdNestActivityLimit = filter.BirdNestActivityLimit,
            DataProvider = filter.DataProvider,
            DataStewardship = filter.DataStewardship,
            Date = filter.Date,
            DeterminationFilter = filter.DeterminationFilter,
            DiffusionStatuses = filter.DiffusionStatuses,
            Event = filter.Event,
            ExcludeFilter = filter.ExcludeFilter,
            Geographics = filter.Geographics,
            ModifiedDate = filter.ModifiedDate,
            NotRecoveredFilter = filter.NotRecoveredFilter,
            ObservedByMe = filter.ObservedByMe,
            OccurrenceStatus = filter.OccurrenceStatus,
            ProjectIds = filter.ProjectIds,
            ReportedByMe = filter.ReportedByMe,
            Taxon = filter.Taxon,
            VerificationStatus = filter.VerificationStatus,
            Output = filter.Output is null
                ? null
                : new OutputFilterDto
                {
                    FieldSet = filter.Output.FieldSet,
                    Fields = filter.Output.Fields
                }
        };
    }
}
