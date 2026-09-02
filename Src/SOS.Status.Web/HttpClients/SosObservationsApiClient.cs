using ContractsModels = SOS.Api.Contracts.Status.Requests;
using SearchFilterInternalDto = SOS.Api.Contracts.Observations.Requests.SearchFilterInternalDto;
using CSharpFunctionalExtensions;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Dtos.Status;
using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.Client.Models;
using System.Text;
using System.Text.Json.Serialization;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Status.Web.HttpClients;

public class SosObservationsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SosObservationsApiClient> _logger;
    private System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public SosObservationsApiClient(HttpClient httpClient, ILogger<SosObservationsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PagedResultDto<Observation>?> SearchObservationsAsync(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        var url = $"/observations/internal/search?skip={skip}&take={take}&sortBy={sortBy}&sortOrder={sortOrder}&validateSearchFilter={validateSearchFilter}&translationCultureCode={translationCultureCode}&sensitiveObservations={sensitiveObservations}";
        var response = await _httpClient.PostAsJsonAsync(url, filter, _jsonSerializerOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
    }

    public async Task<(string GeoJson, long? ObservationsCount, long ResponseSizeBytes)> AdaptiveSearchGeoJsonByLocationAsync(
        SearchFilterInternalDto filter,
        bool cluster = true,
        string clusteringAlgorithm = "Greedy",
        CreatePolygonsMode createPolygons = CreatePolygonsMode.No,
        double clusterRadiusPx = 40.0,
        int minLocationsPerCluster = 2,
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false,
        double? metersPerPixel = null)
    {
        var queryParts = new List<string>
        {
            "outputFormat=GeoJsonByLocationFlat",
            $"cluster={cluster.ToString().ToLower()}",
            $"clusteringAlgorithm={Uri.EscapeDataString(clusteringAlgorithm)}",
            $"createPolygons={createPolygons}",
            $"clusterRadiusPx={clusterRadiusPx.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"minLocationsPerCluster={minLocationsPerCluster}",
            $"validateSearchFilter={validateSearchFilter.ToString().ToLower()}",
            $"translationCultureCode={translationCultureCode}",
            $"sensitiveObservations={sensitiveObservations.ToString().ToLower()}"
        };

        if (metersPerPixel.HasValue)
        {
            queryParts.Add($"metersPerPixel={metersPerPixel.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        var url = $"/observations/internal/adaptivesearch?{string.Join("&", queryParts)}";

        var response = await _httpClient.PostAsJsonAsync(url, filter, _jsonSerializerOptions);
        response.EnsureSuccessStatusCode();

        long? observationsCount = null;
        if (response.Headers.TryGetValues("X-Observations-Count", out var countValues) &&
            long.TryParse(countValues.FirstOrDefault(), out var parsedObservationsCount))
        {
            observationsCount = parsedObservationsCount;
        }

        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var responseSizeBytes = response.Content.Headers.ContentLength ?? responseBytes.LongLength;
        var geoJson = Encoding.UTF8.GetString(responseBytes);

        return (geoJson, observationsCount, responseSizeBytes);
    }    

    public async Task<(string GeoJson, long? ObservationsTotalCount, long? EstimatedLocationCount, long? FeatureCount, long? LocationsBeforeClustering, long ResponseSizeBytes, string? CacheStatus, string? ResultType, string? Timings)> AdaptiveSearchLocationItemsCachedGeoJsonAsync(
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
        var queryParts = new List<string>
        {
            $"observationsLimit={observationsLimit}",
            $"maxGridCells={maxGridCells}",
            $"adaptiveSearchStrategy={adaptiveSearchStrategy}",
            $"cluster={cluster.ToString().ToLower()}",
            $"clusteringAlgorithm={Uri.EscapeDataString(clusteringAlgorithm)}",
            $"createPolygons={createPolygons}",
            $"clusterRadiusPx={clusterRadiusPx.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"minLocationsPerCluster={minLocationsPerCluster}",
            $"minLocationCountBeforeClustering={minLocationCountBeforeClustering}",
            $"validateSearchFilter={validateSearchFilter.ToString().ToLower()}",
            $"translationCultureCode={translationCultureCode}",
            $"sensitiveObservations={sensitiveObservations.ToString().ToLower()}",
            $"createSampleObservations={createSampleObservations.ToString().ToLower()}",
            $"includeFilterAreasInResult={includeFilterAreasInResult.ToString().ToLower()}",
            $"useCache={useCache.ToString().ToLower()}"
        };

        if (metersPerPixel.HasValue)
        {
            queryParts.Add($"metersPerPixel={metersPerPixel.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        if (!string.IsNullOrWhiteSpace(bbox))
        {
            queryParts.Add($"bbox={Uri.EscapeDataString(bbox)}");
        }

        if (cutoffYear.HasValue)
        {
            queryParts.Add($"cutoffYear={cutoffYear.Value}");
        }

        if (locationAggregationLimit.HasValue)
        {
            queryParts.Add($"locationAggregationLimit={locationAggregationLimit.Value}");
        }

        var url = $"/observations/internal/adaptivesearchlocations?{string.Join("&", queryParts)}";

        var response = await _httpClient.PostAsJsonAsync(url, filter, _jsonSerializerOptions);
        response.EnsureSuccessStatusCode();

        long? observationsTotalCount = null;
        if (response.Headers.TryGetValues("X-Observations-TotalCount", out var totalCountValues) &&
            long.TryParse(totalCountValues.FirstOrDefault(), out var parsedObservationsTotalCount))
        {
            observationsTotalCount = parsedObservationsTotalCount;
        }

        long? estimatedLocationCount = null;
        if (response.Headers.TryGetValues("X-Estimated-Location-Count", out var estimatedLocationCountValues) &&
            long.TryParse(estimatedLocationCountValues.FirstOrDefault(), out var parsedEstimatedLocationCount))
        {
            estimatedLocationCount = parsedEstimatedLocationCount;
        }

        long? featureCount = null;
        if (response.Headers.TryGetValues("X-Feature-Count", out var featureCountValues) &&
            long.TryParse(featureCountValues.FirstOrDefault(), out var parsedFeatureCount))
        {
            featureCount = parsedFeatureCount;
        }

        long? locationsBeforeClustering = null;
        if (response.Headers.TryGetValues("X-Locations-Before-Clustering", out var locationsBeforeClusteringValues) &&
            long.TryParse(locationsBeforeClusteringValues.FirstOrDefault(), out var parsedLocationsBeforeClustering))
        {
            locationsBeforeClustering = parsedLocationsBeforeClustering;
        }

        var cacheStatus = response.Headers.TryGetValues("X-Cache", out var cacheStatusValues)
            ? cacheStatusValues.FirstOrDefault()
            : null;

        var resultType = response.Headers.TryGetValues("X-Result-Type", out var resultTypeValues)
            ? resultTypeValues.FirstOrDefault()
            : null;

        var timings = response.Headers.TryGetValues("X-AdaptiveSearch-Timings", out var timingsValues)
            ? timingsValues.FirstOrDefault()
            : null;

        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var responseSizeBytes = response.Content.Headers.ContentLength ?? responseBytes.LongLength;
        var geoJson = Encoding.UTF8.GetString(responseBytes);

        return (geoJson, observationsTotalCount, estimatedLocationCount, featureCount, locationsBeforeClustering, responseSizeBytes, cacheStatus, resultType, timings);
    }

    public async Task<Result<SearchByCursorResultDto<Observation>?>> SearchObservationsByCursorAsync(
        SearchFilterInternalDto filter,
        int take = 1000,
        string? cursor = null,
        string sortBy = "taxon.id",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        try
        {
            var url = $"/observations/searchbycursor?take={take}&sortBy={Uri.EscapeDataString(sortBy)}&sortOrder={sortOrder}&validateSearchFilter={validateSearchFilter}&translationCultureCode={translationCultureCode}&sensitiveObservations={sensitiveObservations}";
            if (!string.IsNullOrEmpty(cursor))
            {
                url += $"&cursor={cursor}";
            }        
            var response = await _httpClient.PostAsJsonAsync(url, filter, _jsonSerializerOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"HTTP {(int)response.StatusCode} ({response.StatusCode}): {errorContent}";
                _logger.LogError("SearchByCursor request failed: {ErrorMessage}", errorMessage);
                return Result.Failure<SearchByCursorResultDto<Observation>?>(errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>(_jsonSerializerOptions);
            return Result.Success(result);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = $"HTTP request failed: {ex.Message}";
            _logger.LogError(ex, "SearchByCursor HTTP request exception: {ErrorMessage}", errorMessage);
            return Result.Failure<SearchByCursorResultDto<Observation>?>(errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error: {ex.Message}";
            _logger.LogError(ex, "SearchByCursor unexpected exception: {ErrorMessage}", errorMessage);
            return Result.Failure<SearchByCursorResultDto<Observation>?>(errorMessage);
        }
    }

    public async Task<Client.Dtos.ProcessSummaryDto?> GetProcessSummary()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Client.Dtos.ProcessSummaryDto>($"systems/process-summary");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching process summary: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<Client.Dtos.ProcessInfoDto?> GetProcessInfo(bool active)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Client.Dtos.ProcessInfoDto>($"Systems/ProcessInformation?active={active}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching process information: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<List<Client.Dtos.DataProviderStatusDto>?> GetDataProviderStatus()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Client.Dtos.DataProviderStatusDto>>("Systems/dataproviderstatus");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching data provider status: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<Client.Dtos.MongoDbProcessInfoDto>?> GetProcessInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Client.Dtos.MongoDbProcessInfoDto>>("systems/processmongodb");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching process information: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<HarvestInfoDto>?> GetHarvestInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HarvestInfoDto>>("systems/harvest");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching harvest information: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<ActiveInstanceInfoDto?> GetActiveInstance()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ActiveInstanceInfoDto>("systems/activeinstance");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching active instance: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<HangfireJobDto>?> GetProcessing()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HangfireJobDto>>("systems/processing");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching processing jobs: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<SearchIndexInfoDto?> GetSearchIndexInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SearchIndexInfoDto>("systems/searchindex");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching search index info: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<MongoDbInfoDto?> GetMongoDatabaseInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MongoDbInfoDto>("systems/mongoinfo");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching MongoDB info: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<HealthReportDto?> GetHealthAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<HealthReportDto>("health");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching health: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        ContractsModels.TaxonRelationsTreeIterationMode treeIterationMode = ContractsModels.TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        string translationCultureCode = "sv-SE")
    {
        try
        {
            var query = string.Join("&", taxonIds.Select(id => $"taxonIds={id}"));
            var url = $"systems/TaxonRelationsDiagram?{query}" +
                      $"&treeIterationMode={treeIterationMode}" +
                      $"&includeSecondaryRelations={includeSecondaryRelations.ToString().ToLower()}" +
                      $"&diagramFormat=Mermaid" + // always Mermaid
                      $"&translationCultureCode={translationCultureCode}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error: {error}";
            }
            else
            {
                return $"Unknown error: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching diagram: {ErrorMsg}", ex.Message);
            return $"Exception: {ex.Message}";
        }
    }
}
