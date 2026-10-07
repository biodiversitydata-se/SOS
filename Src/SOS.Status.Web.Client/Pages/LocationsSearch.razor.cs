﻿﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Status.Web.Client.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SOS.Status.Web.Client.Pages;

public partial class LocationsSearch : IAsyncDisposable
{
    private const string LocationsLayerId = "locations";
    private const string CoordinateUncertaintyLayerId = "coordinate-uncertainty";
    private const string ClusterRadiusLayerId = "cluster-radius";
    private const string ClusterMinimumEnclosingCircleLayerId = "cluster-minimum-enclosing-circle";
    private const int DefaultExampleId = 4;
    private const int AutoSearchDebounceMs = 200;
    private const double ZoomChangeTolerance = 0.01d;
    private const double LocationPointRadiusPx = 5.0d;
    private const double PolygonToPointThresholdPx = LocationPointRadiusPx * 1.25d;
    private const int ObservationPreviewTake = 5;
    private bool showSpinner;
    private bool isLoading;
    private string searchFilter = TaxonSearchFilterSamples.Samples[DefaultExampleId].Json;
    private int? selectedExampleId = DefaultExampleId;
    private string? mapGeoJson;
    private ElementReference _mapContainer;
    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<LocationsSearch>? _dotNetRef;
    private bool mapInitialized;
    private bool cluster = true;
    private bool createSampleObservations;
    private bool useCache = true;
    private string clusteringAlgorithm = "Sweref99TmFastGreedy";
    private readonly string[] clusteringAlgorithms = ["Sweref99TmFastGreedy", "Sweref99TmGreedy", "DbScan"];
    private readonly int[] adaptiveSearchLimitOptions = [250, 500, 1000, 2000, 5000, 10000];
    private readonly int[] locationAggregationLimitOptions = [25, 50, 100, 250, 500, 1000, 2500, 5000];
    private readonly int[] bboxBufferPercentageOptions = [0, 5, 10, 25, 50];
    private string copyMapGeoJsonIcon = Icons.Material.Filled.ContentCopy;
    private int observationsLimit = 5000;
    private int maxGridCells = 1000;
    private int? locationAggregationLimit;
    private AdaptiveSearchLocationsStrategy adaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.ObservationsThenGrid;
    private int? cutoffYear;
    private int bboxBufferPercent = 25;
    private bool includeBoundingBoxInSearch = true;
    private bool searchAutomaticallyWhenMapChanges;
    private bool showCoordinateUncertaintyCircles = false;
    private bool showClusterRadiusCircles;
    private bool showClusterMinimumEnclosingCircles;
    private double clusterRadiusPx = 20.0;
    private int minLocationsPerCluster = 2;
    private int minLocationCountBeforeClustering = 100;
    private CreatePolygonsMode createPolygonsMode = CreatePolygonsMode.Yes;
    private int mapFeatureCount;
    private string currentBboxText = "-";
    private string currentMetersPerPixelText = "-";    
    private double currentZoomLevel = 10;
    private string lastSearchDurationText = "-";
    private string lastResponseSizeText = "-";
    private string observationsTotalCountText = "-";
    private string estimatedLocationCountText = "-";
    private string locationsBeforeClusteringText = "-";
    private string lastCacheStatusText = "-";
    private string lastResultTypeText = "-";
    private string serverTimingsText = "-";
    private string? lastResultType;
    private (double Low, double High)? geogridObservationCountBreaks;
    private MapExtentBounds? lastObservationPreviewExtent;
    private double? lastSearchZoomLevel;
    private string? lastSearchStateSignature;
    private MapExtentBounds? currentVisibleExtent;
    private MapExtentBounds? lastFetchedExtent;
    private CancellationTokenSource? autoSearchDebounceCts;
    private List<SelectedMapFeature> selectedFeatures = [];
    private IReadOnlyList<string> selectedLocationIds = [];
    private IReadOnlyList<string> selectedLocalities = [];
    private readonly Dictionary<string, Dictionary<string, object>> featuresById = [];
    private readonly HashSet<string> mapLocationIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> mapFeatureIdsByLocationId = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Observation> highlightedObservations = [];
    private PagedResultDto<Observation>? observationSearchResult;
    private PagedResultDto<Observation>? defaultObservationSearchResult;
    private bool isObservationPreviewLoading;
    private int observationPreviewRequestVersion;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private static readonly JsonSerializerOptions SelectedFeatureJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
        WriteIndented = true
    };

    private bool IsSearchDisabled =>
        string.IsNullOrWhiteSpace(searchFilter) ||
        !mapInitialized ||
        isLoading ||
        IsCurrentViewCoveredByLastFetch();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _dotNetRef = DotNetObjectReference.Create(this);
        _mapModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./Pages/LocationsSearch.razor.js");

        await _mapModule.InvokeVoidAsync("initMap", _mapContainer, _dotNetRef, _zoom, _centerLon, _centerLat);
        mapInitialized = true;

        var extent = await GetCurrentMapExtentAsync();
        if (extent is not null)
        {
            UpdateMapDiagnostics(extent);
            await UpdateMetersPerPixelDiagnosticsAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task FetchAndRenderMapLayerAsync()
    {
        if (!mapInitialized || _mapModule is null)
        {
            return;
        }

        var searchResult = await GetObservationsAsync();
        if (searchResult.GeoJson == null)
        {
            return;
        }

        ProcessSearchResultGeoJson(searchResult.GeoJson, searchResult.FeatureCount);
        await RemoveLayerAsync(LocationsLayerId);
        await RemoveLayerAsync(CoordinateUncertaintyLayerId);
        await RemoveClusterRadiusLayersAsync();

        mapGeoJson = searchResult.GeoJson;

        if (string.IsNullOrWhiteSpace(searchResult.GeoJson) || mapFeatureCount == 0)
        {
            return;
        }

        try
        {
            await ConfigureClusterRadiusLayersAsync();
            await ConfigureCoordinateUncertaintyLayerAsync();
            await _mapModule.InvokeVoidAsync(
                "loadGeoJsonLayer",
                LocationsLayerId,
                searchResult.GeoJson,
                lastResultType,
                geogridObservationCountBreaks?.Low,
                geogridObservationCountBreaks?.High);
            await SetLayerZIndexAsync(LocationsLayerId, 20);
            await ConfigurePolygonRenderingAsync();
        }
        catch (TaskCanceledException)
        {
            Snackbar.Add("Kartlagret kunde inte uppdateras. Prova att soka igen.");
        }
    }

    [JSInvokable]
    public async Task OnFeaturesClickedFromJs(List<MapFeatureClickItem>? features)
    {
        if (isLoading)
        {
            return;
        }

        var clickedFeatures = new List<SelectedMapFeature>();
        if (features?.Count > 0)
        {
            foreach (var feature in features)
            {
                var properties = ResolveClickedFeatureProperties(feature);
                clickedFeatures.Add(new SelectedMapFeature(feature.FeatureId, properties));
            }
        }

        selectedFeatures = IsSameMapFeatureSelection(selectedFeatures, clickedFeatures)
            ? []
            : clickedFeatures;
        selectedLocationIds = [];
        selectedLocalities = [];

        if (selectedFeatures.Count > 0)
        {
            selectedLocationIds = selectedFeatures
                .SelectMany(feature => ExtractLocationIds(feature.Properties))
                .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(locationId => locationId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            selectedLocalities = selectedFeatures
                .SelectMany(feature => ExtractLocalities(feature.Properties))
                .Where(locality => !string.IsNullOrWhiteSpace(locality))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(locality => locality, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        await ClearObservationHighlightsAsync();
        await SearchObservationPreviewAsync();
    }

    private static bool IsSameMapFeatureSelection(
        IReadOnlyCollection<SelectedMapFeature> currentSelection,
        IReadOnlyCollection<SelectedMapFeature> clickedFeatures)
    {
        if (currentSelection.Count == 0 || currentSelection.Count != clickedFeatures.Count)
        {
            return false;
        }

        var currentFeatureIds = currentSelection
            .Select(feature => feature.FeatureId)
            .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
            .Select(featureId => featureId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var clickedFeatureIds = clickedFeatures
            .Select(feature => feature.FeatureId)
            .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
            .Select(featureId => featureId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return currentFeatureIds.Count == currentSelection.Count &&
               clickedFeatureIds.Count == clickedFeatures.Count &&
               currentFeatureIds.SetEquals(clickedFeatureIds);
    }

    private double _zoom = 14;
    private double _centerLon = 15.00492;
    private double _centerLat = 58.00647;

    [JSInvokable]
    public async Task OnExtentChangedFromJs(MapExtent value)
    {
        UpdateMapDiagnostics(value);
        await UpdateMetersPerPixelDiagnosticsAsync();
        ScheduleAutoSearchIfNeeded();
        await InvokeAsync(StateHasChanged);
    }

    private async Task<(string? GeoJson, long? FeatureCount)> GetObservationsAsync()
    {
        if (string.IsNullOrWhiteSpace(searchFilter) || !mapInitialized)
        {
            return (null, null);
        }

        if (currentVisibleExtent is null)
        {
            var currentExtent = await GetCurrentMapExtentAsync();
            if (currentExtent is null)
            {
                return (null, null);
            }

            UpdateMapDiagnostics(currentExtent);
        }

        var visibleExtent = currentVisibleExtent!.Value;
        var requestedExtent = ExpandExtent(visibleExtent, bboxBufferPercent / 100d);
        SearchFilterInternalDto? searchFilterInternal;
        SearchFilterInternalDto? observationSearchFilterInternal;
        MapExtentBounds? observationPreviewExtent = null;
        var bboxQuery = includeBoundingBoxInSearch ? FormatBoundingBoxQueryValue(requestedExtent) : null;
        try
        {
            searchFilterInternal = JsonSerializer.Deserialize<SearchFilterInternalDto>(searchFilter, jsonSerializerOptions);
            observationSearchFilterInternal = JsonSerializer.Deserialize<SearchFilterInternalDto>(searchFilter, jsonSerializerOptions);
            if (searchFilterInternal == null || observationSearchFilterInternal == null)
            {
                Snackbar.Add("Invalid search filter.");
                return (null, null);
            }

            if (searchFilterInternal.Geographics is not null)
            {
                searchFilterInternal.Geographics.BoundingBox = null;
            }

            observationPreviewExtent = includeBoundingBoxInSearch ? requestedExtent : null;
            ConfigureObservationPreviewFilter(observationSearchFilterInternal, observationPreviewExtent);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Invalid search filter. {ex.Message}");
            return (null, null);
        }

        try
        {
            isLoading = true;
            showSpinner = true;
            observationPreviewRequestVersion++;
            isObservationPreviewLoading = false;
            observationSearchResult = null;
            defaultObservationSearchResult = null;
            selectedFeatures = [];
            selectedLocationIds = [];
            selectedLocalities = [];
            await ClearObservationHighlightsAsync();
            await InvokeAsync(StateHasChanged);

            var stopwatch = Stopwatch.StartNew();
            var metersPerPixel = await GetMetersPerPixelAsync();
            if (createPolygonsMode == CreatePolygonsMode.WhenNeeded && !metersPerPixel.HasValue)
            {
                Snackbar.Add("Kunde inte läsa metersPerPixel från kartan. Välj ett annat polygonläge eller försök igen.");
                return (null, null);
            }

            var searchZoomLevel = currentZoomLevel;

            var searchResult = await searchService.AdaptiveSearchLocationItemsCachedGeoJson(
                searchFilterInternal,
                observationsLimit,
                maxGridCells,
                locationAggregationLimit,
                adaptiveSearchStrategy,
                cluster,
                clusteringAlgorithm,
                createPolygonsMode,
                clusterRadiusPx,
                minLocationsPerCluster,
                minLocationCountBeforeClustering,
                metersPerPixel: metersPerPixel,
                bbox: bboxQuery,
                createSampleObservations: createSampleObservations,
                useCache: useCache,
                cutoffYear: cutoffYear);

            lastResponseSizeText = FormatResponseSize(searchResult.ResponseSizeBytes);
            observationsTotalCountText = searchResult.ObservationsTotalCount?.ToString("N0") ?? "-";
            estimatedLocationCountText = searchResult.EstimatedLocationCount?.ToString("N0") ?? "-";
            locationsBeforeClusteringText = searchResult.LocationsBeforeClustering?.ToString("N0") ?? "-";
            lastCacheStatusText = FormatCacheStatus(searchResult.CacheStatus);
            lastResultType = NormalizeResultType(searchResult.ResultType);
            lastResultTypeText = FormatResultType(lastResultType);
            serverTimingsText = string.IsNullOrWhiteSpace(searchResult.Timings) ? "-" : searchResult.Timings;
            lastObservationPreviewExtent = observationPreviewExtent;

            stopwatch.Stop();
            lastSearchDurationText = $"{stopwatch.Elapsed.TotalSeconds:F2} s";
            lastFetchedExtent = includeBoundingBoxInSearch ? requestedExtent : GetWorldExtent();            
            lastSearchZoomLevel = searchZoomLevel;
            lastSearchStateSignature = GetCurrentSearchStateSignature();

            try
            {
                defaultObservationSearchResult = await searchService.SearchObservations(
                    observationSearchFilterInternal,
                    skip: 0,
                    take: ObservationPreviewTake);
                observationSearchResult = defaultObservationSearchResult;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Kartan uppdaterades, men observationerna kunde inte hämtas. {ex.Message}");
            }

            return (searchResult.GeoJson, searchResult.FeatureCount);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error when calling search service. {ex.Message}");
            return (null, null);
        }
        finally
        {
            isLoading = false;
            showSpinner = false;
            StateHasChanged();
        }
    }

    private async Task OnSearchButtonClickedAsync()
    {
        CancelPendingAutoSearch();
        await FetchAndRenderMapLayerAsync();
    }

    private async Task CopyMapGeoJsonToClipboardAsync()
    {
        if (string.IsNullOrWhiteSpace(mapGeoJson))
        {
            Snackbar.Add("Det finns inget GeoJSON-resultat att kopiera.");
            return;
        }

        try
        {
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", mapGeoJson);
            copyMapGeoJsonIcon = Icons.Material.Filled.Check;
            Snackbar.Add("GeoJSON kopierades till urklipp.");
            await InvokeAsync(StateHasChanged);

            await Task.Delay(1000);
            copyMapGeoJsonIcon = Icons.Material.Filled.ContentCopy;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            copyMapGeoJsonIcon = Icons.Material.Filled.ContentCopy;
            Snackbar.Add($"Kunde inte kopiera GeoJSON. {ex.Message}");
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task OnSearchAutomaticallyWhenMapChangesChanged(bool value)
    {
        searchAutomaticallyWhenMapChanges = value;
        if (searchAutomaticallyWhenMapChanges)
        {
            ScheduleAutoSearchIfNeeded();
        }
        else
        {
            CancelPendingAutoSearch();
        }

        return Task.CompletedTask;
    }

    private void ScheduleAutoSearchIfNeeded()
    {
        if (!searchAutomaticallyWhenMapChanges || currentVisibleExtent == null)
        {
            return;
        }

        CancelPendingAutoSearch();

        var cts = new CancellationTokenSource();
        autoSearchDebounceCts = cts;
        _ = RunAutoSearchAfterDebounceAsync(cts);
    }

    private async Task RunAutoSearchAfterDebounceAsync(CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(AutoSearchDebounceMs, cts.Token);

            while (isLoading)
            {
                await Task.Delay(AutoSearchDebounceMs, cts.Token);
            }

            await InvokeAsync(async () =>
            {
                if (cts.IsCancellationRequested || !ShouldStartAutoSearch())
                {
                    return;
                }

                await FetchAndRenderMapLayerAsync();
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            if (ReferenceEquals(autoSearchDebounceCts, cts))
            {
                autoSearchDebounceCts = null;
            }

            cts.Dispose();
        }
    }

    private bool ShouldStartAutoSearch()
    {
        return searchAutomaticallyWhenMapChanges &&
               !isLoading &&
               mapInitialized &&
               currentVisibleExtent is not null &&
               !string.IsNullOrWhiteSpace(searchFilter) &&
               !IsCurrentViewCoveredByLastFetch();
    }

    private void CancelPendingAutoSearch()
    {
        autoSearchDebounceCts?.Cancel();
        autoSearchDebounceCts = null;
    }

    private async Task OnShowCoordinateUncertaintyCirclesChanged(bool value)
    {
        showCoordinateUncertaintyCircles = value;
        await SetCoordinateUncertaintyLayerVisibleAsync();
    }

    private async Task OnShowClusterRadiusCirclesChanged(bool value)
    {
        showClusterRadiusCircles = value;
        await SetClusterRadiusLayerVisibleAsync(ClusterRadiusLayerId, showClusterRadiusCircles);
    }

    private async Task OnShowClusterMinimumEnclosingCirclesChanged(bool value)
    {
        showClusterMinimumEnclosingCircles = value;
        await SetClusterRadiusLayerVisibleAsync(ClusterMinimumEnclosingCircleLayerId, showClusterMinimumEnclosingCircles);
    }

    private Dictionary<string, object> ParseProperties(JsonElement featureElement)
    {
        var properties = new Dictionary<string, object>();
        if (!featureElement.TryGetProperty("properties", out var propertiesElement) || propertiesElement.ValueKind != JsonValueKind.Object)
        {
            return properties;
        }

        foreach (var property in propertiesElement.EnumerateObject())
        {
            properties[property.Name] = ConvertJsonElementToObject(property.Value);
        }

        return properties;
    }

    private static object ConvertJsonElementToObject(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number when value.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => value.EnumerateArray().Select(ConvertJsonElementToObject).ToList(),
            JsonValueKind.Object => value.EnumerateObject().ToDictionary(
                property => property.Name,
                property => ConvertJsonElementToObject(property.Value)),
            _ => value.ToString()
        };
    }

    private void UpdateMapDiagnostics(MapExtent? extent)
    {
        if (extent is null)
        {
            currentVisibleExtent = null;
            currentBboxText = "-";
            currentMetersPerPixelText = "-";
            currentZoomLevel = _zoom;
            return;
        }

        currentVisibleExtent = ToMapExtentBounds(extent);
        currentZoomLevel = extent.Zoom;
        currentBboxText = FormatBounds(currentVisibleExtent.Value);
    }

    private async Task UpdateMetersPerPixelDiagnosticsAsync()
    {
        currentMetersPerPixelText = FormatMetersPerPixel(await GetMetersPerPixelAsync());
    }

    private bool IsCurrentViewCoveredByLastFetch()
    {
        if (currentVisibleExtent == null ||
            lastFetchedExtent == null ||
            lastSearchZoomLevel == null ||
            string.IsNullOrWhiteSpace(lastSearchStateSignature))
        {
            return false;
        }

        return IsExtentWithin(lastFetchedExtent.Value, currentVisibleExtent.Value) &&
               IsZoomLevelCoveredByLastSearch() &&
               string.Equals(lastSearchStateSignature, GetCurrentSearchStateSignature(), StringComparison.Ordinal);
    }

    private bool IsZoomLevelCoveredByLastSearch()
    {
        return lastSearchZoomLevel.HasValue &&
               Math.Abs(currentZoomLevel - lastSearchZoomLevel.Value) <= ZoomChangeTolerance;
    }

    private string GetCurrentSearchStateSignature()
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{searchFilter}|includeBoundingBox={includeBoundingBoxInSearch}|bboxBufferPercent={(includeBoundingBoxInSearch ? bboxBufferPercent : 0)}|observationsLimit={observationsLimit}|maxGridCells={maxGridCells}|locationAggregationLimit={locationAggregationLimit}|adaptiveSearchStrategy={adaptiveSearchStrategy}|cutoffYear={cutoffYear}|cluster={cluster}|sampleObservations={createSampleObservations}|useCache={useCache}|algorithm={clusteringAlgorithm}|polygons={createPolygonsMode}|clusterRadiusPx={clusterRadiusPx}|minimalLocations={minLocationsPerCluster}|minLocationCountBeforeClustering={minLocationCountBeforeClustering}");
    }

    private static MapExtentBounds ToMapExtentBounds(MapExtent extent)
    {
        return new MapExtentBounds(
            Math.Min(extent.X1, extent.X2),
            Math.Min(extent.Y1, extent.Y2),
            Math.Max(extent.X1, extent.X2),
            Math.Max(extent.Y1, extent.Y2));
    }

    private static MapExtentBounds ExpandExtent(MapExtentBounds extent, double factor)
    {
        var widthPadding = extent.Width * factor;
        var heightPadding = extent.Height * factor;

        return new MapExtentBounds(
            Math.Max(-180, extent.X1 - widthPadding),
            Math.Max(-90, extent.Y1 - heightPadding),
            Math.Min(180, extent.X2 + widthPadding),
            Math.Min(90, extent.Y2 + heightPadding));
    }

    private static MapExtentBounds GetWorldExtent()
    {
        return new MapExtentBounds(-180, -90, 180, 90);
    }

    private static bool IsExtentWithin(MapExtentBounds outer, MapExtentBounds inner)
    {
        return inner.X1 >= outer.X1 &&
               inner.X2 <= outer.X2 &&
               inner.Y1 >= outer.Y1 &&
               inner.Y2 <= outer.Y2;
    }

    private static string FormatBoundingBoxQueryValue(MapExtentBounds extent)
    {
        return FormattableString.Invariant($"{extent.X1:G17},{extent.Y1:G17},{extent.X2:G17},{extent.Y2:G17}");
    }

    private static string FormatBounds(MapExtentBounds extent)
    {
        return string.Create(CultureInfo.InvariantCulture, $"[{extent.X1:F5}, {extent.Y1:F5}] - [{extent.X2:F5}, {extent.Y2:F5}]");
    }

    private static string FormatMetersPerPixel(double? metersPerPixel)
    {
        if (!metersPerPixel.HasValue || !double.IsFinite(metersPerPixel.Value) || metersPerPixel.Value <= 0)
        {
            return "-";
        }

        return metersPerPixel.Value >= 100
            ? $"{metersPerPixel.Value:N0}"
            : $"{metersPerPixel.Value:N2}";
    }

    private static string FormatResponseSize(long responseSizeBytes)
    {
        const double bytesPerKilobyte = 1024d;
        const double bytesPerMegabyte = bytesPerKilobyte * 1024d;

        if (responseSizeBytes >= bytesPerMegabyte)
        {
            return $"{responseSizeBytes / bytesPerMegabyte:F2} MB";
        }

        return $"{responseSizeBytes / bytesPerKilobyte:F2} KB";
    }

    private static string FormatCacheStatus(string? cacheStatus)
    {
        return cacheStatus?.ToUpperInvariant() switch
        {
            "HIT" => "Ja",
            "MISS" => "Nej",
            "BYPASS" => "Nej",
            null or "" => "-",
            _ => cacheStatus
        };
    }

    private static string? NormalizeResultType(string? resultType)
    {
        return string.IsNullOrWhiteSpace(resultType)
            ? null
            : resultType.Trim().ToLowerInvariant();
    }

    private static string FormatResultType(string? resultType)
    {
        return resultType switch
        {
            "locations" => "Locations",
            "geogrid" => "Geogrid",
            "observations" => "Observationer",
            null or "" => "-",
            _ => resultType
        };
    }

    private async Task<MapExtent?> GetCurrentMapExtentAsync()
    {
        if (_mapModule is null)
        {
            return null;
        }

        try
        {
            return await _mapModule.InvokeAsync<MapExtent?>("getExtent");
        }
        catch
        {
            return null;
        }
    }

    private async Task<double?> GetMetersPerPixelAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return null;
            }

            return await _mapModule.InvokeAsync<double?>("getMetersPerPixel");
        }
        catch
        {
            return null;
        }
    }

    private async Task ConfigurePolygonRenderingAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync(
                "configurePolygonRendering",
                LocationsLayerId,
                createPolygonsMode.ToString(),
                lastResultType,
                PolygonToPointThresholdPx);
        }
        catch
        {
            // The server-side polygon mode still works if the optional client-side rendering hook is unavailable.
        }
    }

    private async Task ConfigureCoordinateUncertaintyLayerAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync(
                "configureCoordinateUncertaintyLayer",
                CoordinateUncertaintyLayerId,
                mapGeoJson,
                showCoordinateUncertaintyCircles);
        }
        catch
        {
            // The observation layer is still usable if the optional accuracy-circle overlay is unavailable.
        }
    }

    private async Task ConfigureClusterRadiusLayersAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync(
                "configureClusterRadiusLayer",
                ClusterRadiusLayerId,
                mapGeoJson,
                "cluster.radiusMeters",
                showClusterRadiusCircles);

            await _mapModule.InvokeVoidAsync(
                "configureClusterRadiusLayer",
                ClusterMinimumEnclosingCircleLayerId,
                mapGeoJson,
                "cluster.radiusMetersMin",
                showClusterMinimumEnclosingCircles);
        }
        catch
        {
            // The observation layer is still usable if the optional cluster circle overlays are unavailable.
        }
    }

    private async Task SetLayerZIndexAsync(string layerId, int zIndex)
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync("setLayerZIndex", layerId, zIndex);
        }
        catch
        {
            // Layer ordering is a visual enhancement; the map still works if unavailable.
        }
    }

    private async Task SetCoordinateUncertaintyLayerVisibleAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync(
                "setLayerVisible",
                CoordinateUncertaintyLayerId,
                showCoordinateUncertaintyCircles);
        }
        catch
        {
            // Ignore optional overlay failures.
        }
    }

    private async Task SetClusterRadiusLayerVisibleAsync(string layerId, bool visible)
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync(
                "setLayerVisible",
                layerId,
                visible);
        }
        catch
        {
            // Ignore optional overlay failures.
        }
    }

    private async Task RemoveLayerAsync(string layerId)
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync("removeLayer", layerId);
        }
        catch
        {
            // Ignore optional layer cleanup failures.
        }
    }

    private async Task RemoveClusterRadiusLayersAsync()
    {
        try
        {
            if (_mapModule is null)
            {
                return;
            }

            await _mapModule.InvokeVoidAsync("removeLayer", ClusterRadiusLayerId);
            await _mapModule.InvokeVoidAsync("removeLayer", ClusterMinimumEnclosingCircleLayerId);
        }
        catch
        {
            // Ignore optional overlay failures.
        }
    }

    private void ProcessSearchResultGeoJson(string? geoJson, long? featureCount = null)
    {
        mapFeatureCount = 0;
        selectedFeatures = [];
        selectedLocationIds = [];
        selectedLocalities = [];
        geogridObservationCountBreaks = null;
        featuresById.Clear();
        mapLocationIds.Clear();
        mapFeatureIdsByLocationId.Clear();

        if (string.IsNullOrWhiteSpace(geoJson))
        {
            return;
        }

        using var document = JsonDocument.Parse(geoJson);
        if (!document.RootElement.TryGetProperty("features", out var featuresElement) || featuresElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        mapFeatureCount = featureCount.HasValue
            ? (int)Math.Min(featureCount.Value, int.MaxValue)
            : featuresElement.GetArrayLength();
        var geogridObservationCounts = new List<double>();

        foreach (var featureElement in featuresElement.EnumerateArray())
        {
            var featureId = featureElement.TryGetProperty("id", out var idElement)
                ? idElement.ToString()
                : null;

            var properties = ParseProperties(featureElement);
            var featureLocationIds = ExtractLocationIds(properties)
                .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var locationId in featureLocationIds)
            {
                mapLocationIds.Add(locationId);
            }

            if (!string.IsNullOrWhiteSpace(featureId))
            {
                mapLocationIds.Add(featureId);
                AddMapFeatureLocationMapping(featureId, featureId);

                foreach (var locationId in featureLocationIds)
                {
                    AddMapFeatureLocationMapping(locationId, featureId);
                }
            }

            if (string.Equals(lastResultType, "geogrid", StringComparison.OrdinalIgnoreCase) &&
                TryGetNumericProperty(
                    properties,
                    out var observationsCount,
                    "ObservationsCount",
                    "observationsCount",
                    "ObservationCount",
                    "observationCount",
                    "Count",
                    "count"))
            {
                geogridObservationCounts.Add(observationsCount);
            }

            if (string.IsNullOrWhiteSpace(featureId))
            {
                continue;
            }

            featuresById[featureId] = properties;
        }

        geogridObservationCountBreaks = CalculateGeoGridObservationCountBreaks(geogridObservationCounts);
    }

    private static void ConfigureObservationPreviewFilter(
        SearchFilterInternalDto filter,
        MapExtentBounds? extent)
    {
        var sortOrders = filter.Output?.SortOrders;
        filter.Output = new OutputFilterExtendedDto
        {
            FieldSet = OutputFieldSet.Minimum,
            Fields =
            [
                "location.locationId",
                "location.locality"
            ],
            SortOrders = sortOrders
        };

        if (!extent.HasValue)
        {
            if (filter.Geographics is not null)
            {
                filter.Geographics.BoundingBox = null;
            }

            return;
        }

        filter.Geographics ??= new GeographicsFilterDto();
        filter.Geographics.BoundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto
            {
                Longitude = extent.Value.X1,
                Latitude = extent.Value.Y2
            },
            BottomRight = new LatLonCoordinateDto
            {
                Longitude = extent.Value.X2,
                Latitude = extent.Value.Y1
            }
        };
    }

    private async Task SearchObservationPreviewAsync()
    {
        var requestVersion = ++observationPreviewRequestVersion;
        var hasSelectedLocations = selectedLocationIds.Count > 0;

        SearchFilterInternalDto? observationFilter;
        try
        {
            observationFilter = JsonSerializer.Deserialize<SearchFilterInternalDto>(searchFilter, jsonSerializerOptions);
            if (observationFilter is null)
            {
                Snackbar.Add("Invalid search filter.");
                observationSearchResult = defaultObservationSearchResult;
                await InvokeAsync(StateHasChanged);
                return;
            }

            ConfigureObservationPreviewFilter(
                observationFilter,
                hasSelectedLocations ? null : lastObservationPreviewExtent);
            if (hasSelectedLocations)
            {
                observationFilter.Geographics ??= new GeographicsFilterDto();
                observationFilter.Geographics.LocationIds = selectedLocationIds.ToArray();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Invalid search filter. {ex.Message}");
            observationSearchResult = defaultObservationSearchResult;
            await InvokeAsync(StateHasChanged);
            return;
        }

        isObservationPreviewLoading = true;
        observationSearchResult = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await searchService.SearchObservations(
                observationFilter,
                skip: 0,
                take: ObservationPreviewTake);

            if (requestVersion == observationPreviewRequestVersion)
            {
                observationSearchResult = result;
                if (!hasSelectedLocations)
                {
                    defaultObservationSearchResult = result;
                }
            }
        }
        catch (Exception ex)
        {
            if (requestVersion == observationPreviewRequestVersion)
            {
                observationSearchResult = defaultObservationSearchResult;
                var errorPrefix = hasSelectedLocations
                    ? "Observationerna för den markerade kartfeaturen kunde inte hämtas."
                    : "Observationerna kunde inte hämtas.";
                Snackbar.Add($"{errorPrefix} {ex.Message}");
            }
        }
        finally
        {
            if (requestVersion == observationPreviewRequestVersion)
            {
                isObservationPreviewLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private bool CanHighlightObservation(Observation observation)
    {
        var locationId = observation.Location?.LocationId;
        return string.Equals(lastResultType, "locations", StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(locationId) &&
               mapLocationIds.Contains(locationId);
    }

    private void AddMapFeatureLocationMapping(string locationId, string featureId)
    {
        if (!mapFeatureIdsByLocationId.TryGetValue(locationId, out var featureIds))
        {
            featureIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mapFeatureIdsByLocationId[locationId] = featureIds;
        }

        featureIds.Add(featureId);
    }

    private bool IsObservationHighlighted(Observation observation)
    {
        return highlightedObservations.Contains(observation);
    }

    private async Task OnObservationHighlightChangedAsync(Observation observation, bool highlighted)
    {
        if (highlighted && CanHighlightObservation(observation))
        {
            highlightedObservations.Add(observation);
        }
        else
        {
            highlightedObservations.Remove(observation);
        }

        await UpdateObservationHighlightsAsync();
    }

    private async Task ClearObservationHighlightsAsync()
    {
        highlightedObservations.Clear();
        await UpdateObservationHighlightsAsync();
    }

    private async Task UpdateObservationHighlightsAsync()
    {
        if (_mapModule is null)
        {
            return;
        }

        var locationIds = selectedLocationIds
            .Concat(highlightedObservations
            .Select(observation => observation.Location?.LocationId)
            .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
            .Select(locationId => locationId!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var featureIds = selectedFeatures
            .Select(feature => feature.FeatureId)
            .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
            .Select(featureId => featureId!)
            .Concat(locationIds
                .Where(mapFeatureIdsByLocationId.ContainsKey)
                .SelectMany(locationId => mapFeatureIdsByLocationId[locationId]))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        try
        {
            await _mapModule.InvokeVoidAsync("setHighlightedLocationIds", locationIds, featureIds);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (TaskCanceledException)
        {
        }
    }

    private static (double Low, double High)? CalculateGeoGridObservationCountBreaks(IReadOnlyCollection<double> observationCounts)
    {
        var values = observationCounts
            .Where(value => double.IsFinite(value))
            .OrderBy(value => value)
            .ToArray();

        if (values.Length == 0)
        {
            return null;
        }

        var low = GetNearestRank(values, 1d / 3d);
        var high = GetNearestRank(values, 2d / 3d);

        if (low >= high && values[^1] > values[0])
        {
            var range = values[^1] - values[0];
            low = values[0] + range / 3d;
            high = values[0] + range * 2d / 3d;
        }

        return (low, high);
    }

    private static double GetNearestRank(double[] orderedValues, double fraction)
    {
        var index = (int)Math.Clamp(Math.Floor((orderedValues.Length - 1) * fraction), 0, orderedValues.Length - 1);
        return orderedValues[index];
    }

    private static bool TryGetNumericProperty(IReadOnlyDictionary<string, object>? properties, out double value, params string[] propertyNames)
    {
        value = 0;
        if (properties is null)
        {
            return false;
        }

        foreach (var propertyName in propertyNames)
        {
            if (properties.TryGetValue(propertyName, out var propertyValue) &&
                TryConvertToDouble(propertyValue, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryConvertToDouble(object? value, out double result)
    {
        result = 0;
        return value switch
        {
            null => false,
            double doubleValue when double.IsFinite(doubleValue) => TrySetDouble(doubleValue, out result),
            float floatValue when float.IsFinite(floatValue) => TrySetDouble(floatValue, out result),
            decimal decimalValue => TrySetDouble((double)decimalValue, out result),
            int intValue => TrySetDouble(intValue, out result),
            long longValue => TrySetDouble(longValue, out result),
            JsonElement { ValueKind: JsonValueKind.Number } jsonValue when jsonValue.TryGetDouble(out var doubleValue) => TrySetDouble(doubleValue, out result),
            string stringValue => double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result),
            _ => false
        };
    }

    private static bool TrySetDouble(double value, out double result)
    {
        result = value;
        return double.IsFinite(value);
    }

    private string SelectedLocationIdsText => selectedLocationIds.Count == 0
        ? "-"
        : string.Join(Environment.NewLine, selectedLocationIds.Select(id => $"• {id};"));

    private string SelectedLocalitiesText => selectedLocalities.Count == 0
        ? "-"
        : string.Join(Environment.NewLine, selectedLocalities.Select(loc => $"• {loc};"));

    private string ObservationPanelTitle => observationSearchResult is null
        ? "Observationer"
        : $"Observationer ({observationSearchResult.Records.Count:N0} av {observationSearchResult.TotalCount:N0})";

    private string ObservationPreviewLoadingText => selectedLocationIds.Count > 0
        ? "Hämtar observationer för markerade kartfeatures..."
        : "Hämtar observationer...";

    private Dictionary<string, object> ResolveClickedFeatureProperties(MapFeatureClickItem feature)
    {
        if (!string.IsNullOrWhiteSpace(feature.FeatureId) &&
            featuresById.TryGetValue(feature.FeatureId, out var knownProperties))
        {
            return new Dictionary<string, object>(knownProperties);
        }

        return feature.Properties?.ToDictionary(
            property => property.Key,
            property => ConvertJsonElementToObject(property.Value)) ?? [];
    }

    private static IEnumerable<string> ExtractLocationIds(IReadOnlyDictionary<string, object> properties)
    {
        if (TryGetProperty(properties, out var locationsValue, "locations", "Locations"))
        {
            foreach (var locationId in ExtractLocationIdsFromLocationsValue(locationsValue))
            {
                yield return locationId;
            }
        }

        if (TryGetProperty(properties, out var locationIdsValue, "locationIds", "LocationIds"))
        {
            foreach (var locationId in ExtractStringValues(locationIdsValue))
            {
                yield return locationId;
            }
        }

        if (TryGetProperty(properties, out var locationValue, "location", "Location"))
        {
            foreach (var locationId in ExtractLocationIdsFromLocationValue(locationValue))
            {
                yield return locationId;
            }
        }
    }

    private static IEnumerable<string> ExtractLocalities(IReadOnlyDictionary<string, object> properties)
    {
        if (TryGetProperty(properties, out var locationsValue, "locations", "Locations"))
        {
            foreach (var locality in ExtractLocalitiesFromLocationsValue(locationsValue))
            {
                yield return locality;
            }
        }

        if (TryGetProperty(properties, out var locationValue, "location", "Location"))
        {
            foreach (var locality in ExtractLocalitiesFromLocationValue(locationValue))
            {
                yield return locality;
            }
        }
    }

    private static IEnumerable<string> ExtractLocationIdsFromLocationsValue(object? locationsValue)
    {
        if (locationsValue is IReadOnlyDictionary<string, object> locationsProperties)
        {
            if (TryGetProperty(locationsProperties, out var idsValue, "ids", "Ids"))
            {
                foreach (var locationId in ExtractStringValues(idsValue))
                {
                    yield return locationId;
                }
            }

            if (TryGetProperty(locationsProperties, out var locationValue, "location", "Location"))
            {
                foreach (var locationId in ExtractLocationIdsFromLocationValue(locationValue))
                {
                    yield return locationId;
                }
            }
        }

        if (locationsValue is string locationsJson &&
            TryParseJsonElement(locationsJson, out var locationsElement) &&
            locationsElement.ValueKind == JsonValueKind.Object)
        {
            if (TryGetJsonProperty(locationsElement, out var idsElement, "ids", "Ids"))
            {
                foreach (var locationId in ExtractStringValues(idsElement))
                {
                    yield return locationId;
                }
            }

            if (TryGetJsonProperty(locationsElement, out var locationElement, "location", "Location"))
            {
                foreach (var locationId in ExtractLocationIdsFromLocationValue(locationElement))
                {
                    yield return locationId;
                }
            }
        }
    }

    private static IEnumerable<string> ExtractLocalitiesFromLocationsValue(object? locationsValue)
    {
        if (locationsValue is IReadOnlyDictionary<string, object> locationsProperties &&
            TryGetProperty(locationsProperties, out var locationValue, "location", "Location"))
        {
            foreach (var locality in ExtractLocalitiesFromLocationValue(locationValue))
            {
                yield return locality;
            }
        }

        if (locationsValue is string locationsJson &&
            TryParseJsonElement(locationsJson, out var locationsElement) &&
            locationsElement.ValueKind == JsonValueKind.Object &&
            TryGetJsonProperty(locationsElement, out var locationElement, "location", "Location"))
        {
            foreach (var locality in ExtractLocalitiesFromLocationValue(locationElement))
            {
                yield return locality;
            }
        }
    }

    private static IEnumerable<string> ExtractLocationIdsFromLocationValue(object? locationValue)
    {
        if (locationValue is IReadOnlyDictionary<string, object> locationProperties &&
            TryGetProperty(locationProperties, out var locationIdValue, "locationId", "LocationId"))
        {
            foreach (var locationId in ExtractStringValues(locationIdValue))
            {
                yield return locationId;
            }
        }

        if (locationValue is string locationJson &&
            TryParseJsonElement(locationJson, out var locationElement) &&
            locationElement.ValueKind == JsonValueKind.Object &&
            TryGetJsonProperty(locationElement, out var locationIdElement, "locationId", "LocationId"))
        {
            foreach (var locationId in ExtractStringValues(locationIdElement))
            {
                yield return locationId;
            }
        }
    }

    private static IEnumerable<string> ExtractLocalitiesFromLocationValue(object? locationValue)
    {
        if (locationValue is IReadOnlyDictionary<string, object> locationProperties &&
            TryGetProperty(locationProperties, out var localityValue, "locality", "Locality"))
        {
            foreach (var locality in ExtractStringValues(localityValue))
            {
                yield return locality;
            }
        }

        if (locationValue is string locationJson &&
            TryParseJsonElement(locationJson, out var locationElement) &&
            locationElement.ValueKind == JsonValueKind.Object &&
            TryGetJsonProperty(locationElement, out var localityElement, "locality", "Locality"))
        {
            foreach (var locality in ExtractStringValues(localityElement))
            {
                yield return locality;
            }
        }
    }

    private static IEnumerable<string> ExtractStringValues(object? value)
    {
        switch (value)
        {
            case null:
                yield break;
            case string stringValue:
                if (TryParseJsonElement(stringValue, out var parsedElement))
                {
                    foreach (var nestedValue in ExtractStringValues(parsedElement))
                    {
                        yield return nestedValue;
                    }

                    yield break;
                }

                yield return stringValue;
                yield break;
            case JsonElement jsonElement:
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        foreach (var nestedValue in ExtractStringValues(item))
                        {
                            yield return nestedValue;
                        }
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    var stringValue = jsonElement.GetString();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        yield return stringValue;
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.Number ||
                         jsonElement.ValueKind == JsonValueKind.True ||
                         jsonElement.ValueKind == JsonValueKind.False)
                {
                    yield return jsonElement.ToString();
                }
                yield break;
            case IEnumerable<object> values:
                foreach (var item in values)
                {
                    foreach (var nestedValue in ExtractStringValues(item))
                    {
                        yield return nestedValue;
                    }
                }
                yield break;
            default:
                yield return value.ToString() ?? string.Empty;
                yield break;
        }
    }

    private static bool TryGetProperty(IReadOnlyDictionary<string, object> properties, out object? value, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (properties.TryGetValue(propertyName, out value))
            {
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryGetJsonProperty(JsonElement element, out JsonElement value, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool TryParseJsonElement(string value, out JsonElement element)
    {
        element = default;
        if (string.IsNullOrWhiteSpace(value) ||
            !(value.TrimStart().StartsWith("[", StringComparison.Ordinal) ||
              value.TrimStart().StartsWith("{", StringComparison.Ordinal)))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            element = document.RootElement.Clone();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatSelectedFeatureJson(SelectedMapFeature feature)
    {
        return JsonSerializer.Serialize(feature.Properties, SelectedFeatureJsonSerializerOptions);
    }

    private void LoadExample(int? id)
    {
        selectedExampleId = id ?? DefaultExampleId;
        searchFilter = TaxonSearchFilterSamples.Samples[selectedExampleId.Value].Json;
    }

    public async ValueTask DisposeAsync()
    {
        autoSearchDebounceCts?.Cancel();
        autoSearchDebounceCts?.Dispose();
        autoSearchDebounceCts = null;

        if (_mapModule is not null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("destroyMap");
            }
            catch (JSDisconnectedException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            try
            {
                await _mapModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            _mapModule = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    public sealed class MapExtent
    {
        public double X1 { get; set; }

        public double Y1 { get; set; }

        public double X2 { get; set; }

        public double Y2 { get; set; }

        public double Zoom { get; set; }
    }

    public sealed class MapFeatureClickItem
    {
        public string? FeatureId { get; set; }

        public Dictionary<string, JsonElement>? Properties { get; set; }
    }

    private sealed record SelectedMapFeature(
        string? FeatureId,
        Dictionary<string, object> Properties);

    private readonly record struct MapExtentBounds(double X1, double Y1, double X2, double Y2)
    {
        public double Width => Math.Max(0, X2 - X1);

        public double Height => Math.Max(0, Y2 - Y1);
    }
}
