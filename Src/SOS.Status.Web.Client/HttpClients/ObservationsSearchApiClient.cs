using CSharpFunctionalExtensions;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Client.JsonConverters;
using SOS.Status.Web.Client.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Status.Web.Client.HttpClients;

public class ObservationsSearchApiClient : IObservationSearchService
{
    private readonly HttpClient _http;

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoJsonConverter()
        }
    };

    public ObservationsSearchApiClient(HttpClient http)
    {
        _http = http;
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
        var url = $"api/observations/search?" +
            $"skip={skip}" +
            $"&take={take}" +
            $"&sortBy={sortBy}" +
            $"&sortOrder={sortOrder}" +
            $"&validateSearchFilter={validateSearchFilter.ToString().ToLower()}" +
            $"&translationCultureCode={translationCultureCode}" +
            $"&sensitiveObservations={sensitiveObservations.ToString().ToLower()}";

        try
        {
            var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(_jsonSerializerOptions);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, {error}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception: {ex.Message}");
        }
    }

    public async Task<(string GeoJson, long? ObservationsCount, long ResponseSizeBytes)> AdaptiveSearchGeoJsonByLocation(
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

        var url = $"api/observations/adaptivesearch?{string.Join("&", queryParts)}";

        var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error: {response.StatusCode}, {error}");
        }

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

    public async Task<(string GeoJson, long? ObservationsCount, long ResponseSizeBytes)> AdaptiveSearchLocationItemsGeoJson(
        SearchFilterInternalDto filter,
        bool cluster = true,
        string clusteringAlgorithm = "Greedy",
        CreatePolygonsMode createPolygons = CreatePolygonsMode.No,
        double clusterRadiusPx = 40.0,
        int minLocationsPerCluster = 2,
        int minLocationCountBeforeClustering = 100,
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false,
        double? metersPerPixel = null)
    {
        var queryParts = new List<string>
        {
            $"cluster={cluster.ToString().ToLower()}",
            $"clusteringAlgorithm={Uri.EscapeDataString(clusteringAlgorithm)}",
            $"createPolygons={createPolygons}",
            $"clusterRadiusPx={clusterRadiusPx.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"minLocationsPerCluster={minLocationsPerCluster}",
            $"minLocationCountBeforeClustering={minLocationCountBeforeClustering}",
            $"validateSearchFilter={validateSearchFilter.ToString().ToLower()}",
            $"translationCultureCode={translationCultureCode}",
            $"sensitiveObservations={sensitiveObservations.ToString().ToLower()}"
        };

        if (metersPerPixel.HasValue)
        {
            queryParts.Add($"metersPerPixel={metersPerPixel.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        var url = $"api/observations/adaptivesearchlocations?{string.Join("&", queryParts)}";

        var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error: {response.StatusCode}, {error}");
        }

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
        int? cutoffYear = null)
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

        var url = $"api/observations/adaptivesearchlocations?{string.Join("&", queryParts)}";

        var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error: {response.StatusCode}, {error}");
        }

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
        var url = $"api/observations/searchbycursor?" +
            $"take={take}" +
            $"&sortBy={Uri.EscapeDataString(sortBy)}" +
            $"&sortOrder={sortOrder}" +
            $"&validateSearchFilter={validateSearchFilter.ToString().ToLower()}" +
            $"&translationCultureCode={translationCultureCode}" +
            $"&sensitiveObservations={sensitiveObservations.ToString().ToLower()}";
        if (!string.IsNullOrEmpty(cursor))
        {
            url += $"&cursor={cursor}";
        }        

        try
        {
            var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"HTTP {(int)response.StatusCode} ({response.StatusCode}): {errorContent}";
                return Result.Failure<SearchByCursorResultDto<Observation>>(errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>(_jsonSerializerOptions);
            if (result is null)
            {
                return Result.Failure<SearchByCursorResultDto<Observation>>("Response content was empty.");
            }

            return Result.Success(result);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = $"HTTP request failed: {ex.Message}";
            return Result.Failure<SearchByCursorResultDto<Observation>>(errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error: {ex.Message}";
            return Result.Failure<SearchByCursorResultDto<Observation>>(errorMessage);
        }
    }
}
