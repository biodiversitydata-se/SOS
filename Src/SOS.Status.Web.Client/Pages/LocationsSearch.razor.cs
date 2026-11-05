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
    private const int DefaultExampleId = 4;
    private const int AutoSearchDebounceMs = 200;
    private const double ZoomChangeTolerance = 0.01d;
    private const double LocationPointRadiusPx = 5.0d;
    private const double PolygonToPointThresholdPx = LocationPointRadiusPx; // 10 px diameter, matching the API.
    private const int ObservationPreviewTake = 5;
    private bool isLoading;
    private CancellationTokenSource? mapSearchCts;
    private int mapSearchRequestVersion;
    private bool hasStartedManualSearch;
    private Task mapSearchTask = Task.CompletedTask;
    private bool mapSearchPending;
    private string searchFilter = TaxonSearchFilterSamples.Samples[DefaultExampleId].Json;
    private int? selectedExampleId = DefaultExampleId;
    private string? mapGeoJson;
    private ElementReference _mapContainer;
    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<LocationsSearch>? _dotNetRef;
    private bool mapInitialized;
    private bool disposed;
    private bool cluster = true;
    private bool createSampleObservations;
    private bool useCache = true;
    private double? clusterRadiusMeters;
    private double? compactClusterMaxRadiusMeters;
    private PointSizeMode pointSizeMode = PointSizeMode.ByObservationCountFixedBreaks;
    private List<PointSizeLegendItem> pointSizeLegend = [];
    private string? mapSearchFilter;
    private readonly int[] adaptiveSearchLimitOptions = [250, 500, 1000, 2000, 5000, 10000];
    private readonly int[] locationAggregationLimitOptions = [25, 50, 100, 250, 500, 1000, 2500, 5000];
    private readonly int[] bboxBufferPercentageOptions = [0, 5, 10, 25, 50];
    private string copyMapGeoJsonIcon = Icons.Material.Filled.ContentCopy;
    private int observationsLimit = 5000;
    private int maxGridCells = 1000;
    private int locationAggregationLimit = 1000;
    private AdaptiveSearchLocationsStrategy adaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.Auto;
    private int? cutoffYear;
    private int bboxBufferPercent = 0;
    private bool includeBoundingBoxInSearch = true;
    private bool searchAutomaticallyWhenMapChanges = true;
    private bool showClickableCursor = true;
    private bool showCoordinateUncertaintyCircles = false;
    private bool showClusterRadiusCircles;
    private bool showClusterMinimumEnclosingCircles;
    private bool showFilterAreaOutlines = true;
    private double clusterRadiusPx = AdaptiveSearchLocationsDefaults.ClusterRadiusPx;
    private double switchToDetailClusteringAtMetersPerPixel = AdaptiveSearchLocationsDefaults.SwitchToDetailClusteringAtMetersPerPixel;
    private double detailClusterRadiusPx = AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx;
    private int minLocationsPerCluster = 2;
    private int minLocationCountBeforeClustering = AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering;
    private CreatePolygonsMode createPolygonsMode = CreatePolygonsMode.WhenNeeded;
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
    private string lastAdaptiveSearchStrategyText = "-";
    private string serverTimingsText = "-";
    private string? lastResultType;
    private MapExtentBounds? lastObservationPreviewExtent;
    private double? lastSearchZoomLevel;
    private MapSearchSettings? lastSearchSettings;
    private MapExtentBounds? currentVisibleExtent;
    private MapExtentBounds? lastFetchedExtent;
    private CancellationTokenSource? autoSearchDebounceCts;
    private List<SelectedMapFeature> selectedFeatures = [];
    private IReadOnlyList<string> selectedLocationIds = [];
    private List<ObservationLocationOption> observationLocationOptions = [];
    private ObservationLocationOption? selectedObservationLocation;
    private string selectedLocationIdsText = "-";
    private string selectedLocalitiesText = "-";
    private string selectedFeatureJson = string.Empty;
    private readonly Dictionary<string, JsonElement> featuresById = [];
    private readonly HashSet<string> mapLocationIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Observation> highlightedObservations = [];
    private PagedResultDto<Observation>? observationSearchResult;
    private bool isObservationPreviewLoading;
    private int observationPreviewRequestVersion;
    private int mapClickVersion;

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
        await _mapModule.InvokeVoidAsync("setClickableCursorEnabled", showClickableCursor);
        mapInitialized = true;
        await ConfigureCompactClusterRenderingAsync();
        await ConfigurePointSizingAsync();

        var extent = await GetCurrentMapExtentAsync();
        if (extent is not null)
        {
            UpdateMapDiagnostics(extent);
            await UpdateMetersPerPixelDiagnosticsAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    private bool IsCurrentMapSearch(int version, CancellationToken cancellationToken) =>
        !disposed && !cancellationToken.IsCancellationRequested && version == mapSearchRequestVersion;

    private bool IsMapSnapshotApplicable(MapSearchSnapshot snapshot, CancellationToken cancellationToken) =>
        !disposed && !cancellationToken.IsCancellationRequested && currentVisibleExtent.HasValue &&
        IsExtentWithin(snapshot.FetchedExtent, currentVisibleExtent.Value) &&
        Math.Abs(currentZoomLevel - snapshot.Zoom) <= ZoomChangeTolerance &&
        snapshot.Settings == GetCurrentSearchSettings();

    private Task FetchAndRenderMapLayerAsync()
    {
        if (!mapInitialized || _mapModule is null || disposed) return Task.CompletedTask;

        mapSearchRequestVersion++;
        mapSearchPending = true;
        if (!isLoading) mapSearchTask = RunPendingMapSearchesAsync();
        return mapSearchTask;
    }

    private async Task RunPendingMapSearchesAsync()
    {
        using var cts = new CancellationTokenSource();
        mapSearchCts = cts;
        var cancellationToken = cts.Token;
        isLoading = true;
        try
        {
            while (mapSearchPending && !disposed)
            {
                mapSearchPending = false;
                var version = mapSearchRequestVersion;
                if (selectedLocationIds.Count == 0)
                {
                    observationPreviewRequestVersion++;
                    isObservationPreviewLoading = false;
                }
                var snapshot = await GetObservationsAsync(cancellationToken, version);
                if (snapshot is null)
                {
                    if (IsCurrentMapSearch(version, cancellationToken) && !mapSearchPending && lastFetchedExtent is null)
                        hasStartedManualSearch = false;
                    continue;
                }
                if (IsMapSnapshotApplicable(snapshot, cancellationToken))
                    await RenderMapSnapshotAsync(snapshot, cancellationToken);
                // Only one HTTP request/render is active. A ready pending search reads the latest
                // settings and viewport on the next iteration; intermediate views are never queued.
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) when (disposed) { }
        catch (TaskCanceledException)
        {
            if (!disposed) Snackbar.Add("Kartlagret kunde inte uppdateras. Prova att söka igen.");
        }
        finally
        {
            mapSearchCts = null;
            isLoading = false;
            if (!disposed) await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RenderMapSnapshotAsync(MapSearchSnapshot snapshot, CancellationToken cancellationToken)
    {
        var mapModule = _mapModule!;
        ApplyMapSearchResult(snapshot);
        ProcessSearchResultGeoJson(snapshot.Result.GeoJson, snapshot.Result.FeatureCount);
        mapGeoJson = snapshot.Result.GeoJson;
        var legend = await mapModule.InvokeAsync<List<PointSizeLegendItem>?>(
            "renderMapResult", LocationsLayerId, snapshot.Result.GeoJson, new
            {
                resultType = lastResultType,
                createPolygons = createPolygonsMode.ToString(),
                polygonToPointThresholdPx = PolygonToPointThresholdPx,
                pointSizeMode = pointSizeMode.ToString(),
                filterJson = snapshot.Settings.FilterJson,
                showFilterAreaOutlines,
                highlight = CreateMapHighlightState(),
                overlays = new
                {
                    coordinateUncertainty = showCoordinateUncertaintyCircles,
                    clusterRadius = showClusterRadiusCircles,
                    clusterMinimumEnclosingCircle = showClusterMinimumEnclosingCircles
                }
            });
        if (legend is null || !IsMapSnapshotApplicable(snapshot, cancellationToken)) return;
        pointSizeLegend = legend;
        lastFetchedExtent = snapshot.FetchedExtent;
        lastSearchZoomLevel = snapshot.Zoom;
        lastSearchSettings = snapshot.Settings;
        // This result also satisfies any pending view contained in its bbox at this resolution.
        CancelPendingAutoSearch();
        if (selectedLocationIds.Count == 0)
        {
            if (snapshot.Result.ObservationsTotalCount == 0)
            {
                // The map already established that this viewport has no observations.
                observationPreviewRequestVersion++;
                isObservationPreviewLoading = false;
                observationSearchResult = new PagedResultDto<Observation> { Take = ObservationPreviewTake, TotalCount = 0 };
                await ClearObservationHighlightsAsync();
            }
            else
            {
                _ = SearchObservationPreviewAsync(snapshot.PreviewFilter);
            }
        }
    }

    [JSInvokable]
    public async Task OnFeaturesClickedFromJs(List<MapFeatureClickItem>? features, int clickVersion = 0)
    {
        if (disposed || (clickVersion > 0 && clickVersion < mapClickVersion)) return;
        mapClickVersion = Math.Max(mapClickVersion, clickVersion);
        if (isLoading)
        {
            await UpdateObservationHighlightsAsync();
            return;
        }
        observationPreviewRequestVersion++;

        var clickedFeatures = new List<SelectedMapFeature>();
        if (features?.Count > 0)
        {
            foreach (var feature in features)
            {
                var properties = ResolveClickedFeatureProperties(feature);
                clickedFeatures.Add(new SelectedMapFeature(feature.FeatureId, properties));
            }
        }

        SetSelectedFeatures(IsSameMapFeatureSelection(selectedFeatures, clickedFeatures)
            ? []
            : clickedFeatures);

        await ClearObservationHighlightsAsync();
        if (disposed || (clickVersion > 0 && clickVersion != mapClickVersion)) return;
        await SearchObservationPreviewAsync();
    }

    private void SetSelectedFeatures(List<SelectedMapFeature> features)
    {
        selectedFeatures = features;
        selectedObservationLocation = null;
        observationLocationOptions = [];
        if (features.Count == 0)
        {
            selectedLocationIds = [];
            selectedLocationIdsText = "-";
            selectedLocalitiesText = "-";
            selectedFeatureJson = string.Empty;
            return;
        }

        selectedLocationIds = features
            .SelectMany(feature => ExtractLocationIds(feature.Properties))
            .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(locationId => locationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        observationLocationOptions = CreateObservationLocationOptions();
        selectedObservationLocation = observationLocationOptions[0];
        var localities = features
            .SelectMany(feature => ExtractLocalities(feature.Properties))
            .Where(locality => !string.IsNullOrWhiteSpace(locality))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(locality => locality, StringComparer.OrdinalIgnoreCase);
        var itemCount = features.Sum(feature =>
        {
            var items = GetLocationArray(feature.Properties, "items");
            return items.ValueKind == JsonValueKind.Array ? items.GetArrayLength() : 0;
        });

        // Prepare the panel once per selection; map movement and table updates reuse its text.
        selectedLocationIdsText = selectedLocationIds.Count == 0 ? "-"
            : string.Join(Environment.NewLine, selectedLocationIds.Select(id => $"• {id};"));
        selectedLocalitiesText = string.Join(Environment.NewLine, localities.Select(locality => $"• {locality};")) +
            $"{Environment.NewLine}{itemCount} av {selectedLocationIds.Count} lokaler";
        selectedFeatureJson = features[0].HasProperties
            ? JsonSerializer.Serialize(features[0].Properties, SelectedFeatureJsonSerializerOptions)
            : string.Empty;
    }

    private sealed record ObservationLocationOption(string? LocationId, string Label);

    private string AllObservationLocationsLabel => $"Alla lokaler ({selectedLocationIds.Count:N0})";

    private IReadOnlyList<string> ObservationPreviewLocationIds => selectedObservationLocation?.LocationId is { } id
        ? [id]
        : selectedLocationIds;

    private List<ObservationLocationOption> CreateObservationLocationOptions()
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var feature in selectedFeatures)
        {
            var items = GetLocationArray(feature.Properties, "items");
            if (items.ValueKind != JsonValueKind.Array) continue;
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("locationId", out var locationId) && locationId.ValueKind == JsonValueKind.String &&
                    locationId.GetString() is { } id && !string.IsNullOrWhiteSpace(id) &&
                    item.TryGetProperty("locality", out var locality) && locality.ValueKind == JsonValueKind.String &&
                    locality.GetString() is { } name && !string.IsNullOrWhiteSpace(name))
                {
                    names.TryAdd(id, name.Trim());
                }
            }
        }

        return [new(null, AllObservationLocationsLabel), .. selectedLocationIds
            .Select(id => new ObservationLocationOption(id, names.TryGetValue(id, out var name) ? $"{name} ({id})" : id))
            .OrderBy(option => option.Label, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(option => option.LocationId, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task OnObservationLocationChangedAsync(ObservationLocationOption? option)
    {
        if (disposed || (option is not null && !observationLocationOptions.Contains(option))) return;
        var previousId = selectedObservationLocation?.LocationId;
        selectedObservationLocation = option;
        if (string.Equals(previousId, option?.LocationId, StringComparison.OrdinalIgnoreCase)) return;

        var requestVersion = ++observationPreviewRequestVersion;
        observationSearchResult = null;
        highlightedObservations.Clear();
        isObservationPreviewLoading = true;
        await UpdateObservationHighlightsAsync();
        if (disposed || requestVersion != observationPreviewRequestVersion) return;
        // Finish the dropdown selection immediately; the versioned search handles its own errors.
        _ = SearchObservationPreviewAsync();
    }

    private static bool IsSameMapFeatureSelection(
        IReadOnlyCollection<SelectedMapFeature> currentSelection,
        IReadOnlyCollection<SelectedMapFeature> clickedFeatures)
    {
        if (currentSelection.Count == 0 || currentSelection.Count != clickedFeatures.Count)
        {
            return false;
        }

        var currentLocationIds = currentSelection
            .SelectMany(feature => ExtractLocationIds(feature.Properties))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var clickedLocationIds = clickedFeatures
            .SelectMany(feature => ExtractLocationIds(feature.Properties))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (currentLocationIds.Count > 0 || clickedLocationIds.Count > 0)
        {
            return currentLocationIds.SetEquals(clickedLocationIds);
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

    private async Task<MapSearchSnapshot?> GetObservationsAsync(CancellationToken cancellationToken, int version)
    {
        if (string.IsNullOrWhiteSpace(searchFilter) || !mapInitialized)
        {
            return null;
        }

        if (currentVisibleExtent is null)
        {
            var currentExtent = await GetCurrentMapExtentAsync();
            if (currentExtent is null)
            {
                return null;
            }

            UpdateMapDiagnostics(currentExtent);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var settings = GetCurrentSearchSettings();
        var filterJson = settings.FilterJson;
        var visibleExtent = currentVisibleExtent!.Value;
        var requestedExtent = ExpandExtent(visibleExtent, settings.BboxBufferPercent / 100d);
        SearchFilterInternalDto? searchFilterInternal;
        SearchFilterInternalDto? observationSearchFilterInternal;
        MapExtentBounds? observationPreviewExtent = null;
        var bboxQuery = settings.IncludeBoundingBox ? FormatBoundingBoxQueryValue(requestedExtent) : null;
        try
        {
            searchFilterInternal = JsonSerializer.Deserialize<SearchFilterInternalDto>(filterJson, jsonSerializerOptions);
            observationSearchFilterInternal = JsonSerializer.Deserialize<SearchFilterInternalDto>(filterJson, jsonSerializerOptions);
            if (searchFilterInternal == null || observationSearchFilterInternal == null)
            {
                Snackbar.Add("Invalid search filter.");
                return null;
            }

            if (searchFilterInternal.Geographics is not null)
            {
                searchFilterInternal.Geographics.BoundingBox = null;
            }

            observationPreviewExtent = settings.IncludeBoundingBox ? requestedExtent : null;
            ConfigureObservationPreviewFilter(observationSearchFilterInternal, observationPreviewExtent);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Invalid search filter. {ex.Message}");
            return null;
        }

        try
        {
            var searchZoomLevel = currentZoomLevel;
            var fetchedExtent = settings.IncludeBoundingBox ? requestedExtent : GetWorldExtent();
            await InvokeAsync(StateHasChanged);

            var stopwatch = Stopwatch.StartNew();
            var metersPerPixel = await GetMetersPerPixelAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsCurrentMapSearch(version, cancellationToken)) return null;
            if (settings.Options.CreatePolygons == CreatePolygonsMode.WhenNeeded && !metersPerPixel.HasValue)
            {
                Snackbar.Add("Kunde inte läsa metersPerPixel från kartan. Välj ett annat polygonläge eller försök igen.");
                return null;
            }

            var options = settings.Options with { Bbox = bboxQuery, MetersPerPixel = metersPerPixel };
            var searchResult = await searchService.AdaptiveSearchLocationItemsCachedGeoJson(searchFilterInternal, options, cancellationToken);

            if (disposed) return null;

            cancellationToken.ThrowIfCancellationRequested();
            return new MapSearchSnapshot(searchResult, observationSearchFilterInternal,
                JsonSerializer.Serialize(searchFilterInternal, jsonSerializerOptions), observationPreviewExtent,
                fetchedExtent, searchZoomLevel, settings, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            if (IsCurrentMapSearch(version, cancellationToken)) Snackbar.Add($"Error when calling search service. {ex.Message}");
            return null;
        }
    }

    private sealed record MapSearchSnapshot(LocationMapSearchResult Result,
        SearchFilterInternalDto PreviewFilter, string FilterJson, MapExtentBounds? PreviewExtent,
        MapExtentBounds FetchedExtent, double Zoom, MapSearchSettings Settings, TimeSpan Duration);

    private sealed record MapSearchSettings(string FilterJson, bool IncludeBoundingBox,
        int BboxBufferPercent, LocationMapSearchOptions Options);

    private void ApplyMapSearchResult(MapSearchSnapshot snapshot)
    {
        var searchResult = snapshot.Result;
        lastResponseSizeText = FormatResponseSize(searchResult.ResponseSizeBytes);
        observationsTotalCountText = searchResult.ObservationsTotalCount?.ToString("N0") ?? "-";
        estimatedLocationCountText = searchResult.EstimatedLocationCount?.ToString("N0") ?? "-";
        var resultSearchFilter = snapshot.FilterJson;
        if (!string.Equals(mapSearchFilter, resultSearchFilter, StringComparison.Ordinal))
        {
            // A changed filter starts a new selection context. Viewport-only searches
            // keep the selected locations and any pending observation request.
            observationPreviewRequestVersion++;
            isObservationPreviewLoading = false;
            SetSelectedFeatures([]);
            highlightedObservations.Clear();
        }
        else if (selectedLocationIds.Count == 0)
        {
            // Grid/area feature ids are tied to one result and cannot identify a location.
            SetSelectedFeatures([]);
        }
        mapSearchFilter = resultSearchFilter;
        locationsBeforeClusteringText = searchResult.LocationsBeforeClustering?.ToString("N0") ?? "-";
        lastCacheStatusText = FormatCacheStatus(searchResult.CacheStatus);
        lastResultType = NormalizeResultType(searchResult.ResultType);
        lastResultTypeText = FormatResultType(lastResultType);
        lastAdaptiveSearchStrategyText = searchResult.AdaptiveSearchStrategy?.ToString() ?? "-";
        serverTimingsText = string.IsNullOrWhiteSpace(searchResult.Timings) ? "-" : searchResult.Timings;
        lastObservationPreviewExtent = snapshot.PreviewExtent;

        lastSearchDurationText = $"{snapshot.Duration.TotalSeconds:F2} s";
    }

    private async Task OnSearchButtonClickedAsync()
    {
        CancelPendingAutoSearch();
        hasStartedManualSearch = true;
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
        // Let the user configure the first search before pan/zoom can start requests.
        if (disposed || !mapInitialized || !searchAutomaticallyWhenMapChanges ||
            (!hasStartedManualSearch && lastFetchedExtent is null) || currentVisibleExtent == null)
        {
            return;
        }

        CancelPendingAutoSearch();
        mapSearchRequestVersion++;
        if (IsCurrentViewCoveredByLastFetch()) return;

        var cts = new CancellationTokenSource();
        autoSearchDebounceCts = cts;
        _ = RunAutoSearchAfterDebounceAsync(cts);
    }

    private async Task RunAutoSearchAfterDebounceAsync(CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(AutoSearchDebounceMs, cts.Token);

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
               (hasStartedManualSearch || lastFetchedExtent is not null) &&
               mapInitialized &&
               currentVisibleExtent is not null &&
               !string.IsNullOrWhiteSpace(searchFilter) &&
               !IsCurrentViewCoveredByLastFetch();
    }

    private void CancelPendingAutoSearch()
    {
        mapSearchPending = false;
        autoSearchDebounceCts?.Cancel();
        autoSearchDebounceCts = null;
    }

    private async Task OnCompactClusterMaxRadiusMetersChanged(double? value)
    {
        compactClusterMaxRadiusMeters = value.HasValue && double.IsFinite(value.Value) && value.Value >= 0
            ? value : null;
        await ConfigureCompactClusterRenderingAsync();
    }

    private async Task OnShowClickableCursorChanged(bool value)
    {
        showClickableCursor = value;
        if (mapInitialized && _mapModule is not null)
            await _mapModule.InvokeVoidAsync("setClickableCursorEnabled", value);
    }

    private async Task OnShowFilterAreaOutlinesChanged(bool value)
    {
        showFilterAreaOutlines = value;
        if (mapInitialized && _mapModule is not null)
            await _mapModule.InvokeVoidAsync("setFilterAreaOutlinesVisible", value);
    }

    [JSInvokable]
    public Task OnFilterAreaLoadFailedFromJs()
    {
        if (!disposed) Snackbar.Add("Sökfiltrets konturer kunde inte hämtas. Prova att söka igen.", Severity.Warning);
        return Task.CompletedTask;
    }

    private async Task ConfigureCompactClusterRenderingAsync()
    {
        if (!mapInitialized || _mapModule is null)
        {
            return;
        }

        await _mapModule.InvokeVoidAsync("configureCompactClusterRendering", compactClusterMaxRadiusMeters);
    }

    private async Task OnPointSizeModeChanged(PointSizeMode value)
    {
        pointSizeMode = value;
        await ConfigurePointSizingAsync();
    }

    private async Task ConfigurePointSizingAsync()
    {
        if (mapInitialized && _mapModule is not null)
        {
            pointSizeLegend = await _mapModule.InvokeAsync<List<PointSizeLegendItem>>(
                "configurePointSizing", pointSizeMode.ToString()) ?? [];
        }
    }

    private bool ShowPointSizeLegend => pointSizeMode != PointSizeMode.Fixed &&
        string.Equals(lastResultType, "locations", StringComparison.OrdinalIgnoreCase) && pointSizeLegend.Count > 0;

    private async Task OnShowCoordinateUncertaintyCirclesChanged(bool value)
    {
        showCoordinateUncertaintyCircles = value;
        await ConfigureLocationOverlaysAsync();
    }

    private async Task OnShowClusterRadiusCirclesChanged(bool value)
    {
        showClusterRadiusCircles = value;
        await ConfigureLocationOverlaysAsync();
    }

    private async Task OnShowClusterMinimumEnclosingCirclesChanged(bool value)
    {
        showClusterMinimumEnclosingCircles = value;
        await ConfigureLocationOverlaysAsync();
    }

    private static JsonElement ParseProperties(JsonElement featureElement)
    {
        // Keep an owned copy so selections remain valid after this result's document is disposed.
        return featureElement.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object
            ? properties.Clone()
            : default;
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
            lastSearchSettings is null)
        {
            return false;
        }

        return IsExtentWithin(lastFetchedExtent.Value, currentVisibleExtent.Value) &&
               IsZoomLevelCoveredByLastSearch() &&
               lastSearchSettings == GetCurrentSearchSettings();
    }

    private bool IsZoomLevelCoveredByLastSearch()
    {
        return lastSearchZoomLevel.HasValue &&
               Math.Abs(currentZoomLevel - lastSearchZoomLevel.Value) <= ZoomChangeTolerance;
    }

    private MapSearchSettings GetCurrentSearchSettings()
    {
        return new MapSearchSettings(searchFilter, includeBoundingBoxInSearch,
            includeBoundingBoxInSearch ? bboxBufferPercent : 0,
            new LocationMapSearchOptions
            {
                ObservationsLimit = observationsLimit,
                MaxGridCells = maxGridCells,
                LocationAggregationLimit = locationAggregationLimit,
                AdaptiveSearchStrategy = adaptiveSearchStrategy,
                Cluster = cluster,
                ClusterRadiusMeters = clusterRadiusMeters,
                CreatePolygons = createPolygonsMode,
                ClusterRadiusPx = clusterRadiusPx,
                MinLocationsPerCluster = minLocationsPerCluster,
                MinLocationCountBeforeClustering = minLocationCountBeforeClustering,
                CreateSampleObservations = createSampleObservations,
                UseCache = useCache,
                CutoffYear = cutoffYear,
                SwitchToDetailClusteringAtMetersPerPixel = switchToDetailClusteringAtMetersPerPixel,
                DetailClusterRadiusPx = detailClusterRadiusPx
            });
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

    private async Task ConfigureLocationOverlaysAsync()
    {
        try
        {
            if (_mapModule is null) return;
            await _mapModule.InvokeVoidAsync(
                "configureLocationOverlays",
                showCoordinateUncertaintyCircles,
                showClusterRadiusCircles,
                showClusterMinimumEnclosingCircles);
        }
        catch
        {
            // Optional overlays must not prevent the observation layer from being used.
        }
    }

    private void ProcessSearchResultGeoJson(string? geoJson, long? featureCount = null)
    {
        mapFeatureCount = 0;
        pointSizeLegend = [];
        featuresById.Clear();
        mapLocationIds.Clear();

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

        foreach (var featureElement in featuresElement.EnumerateArray())
        {
            var featureId = featureElement.TryGetProperty("id", out var idElement)
                ? idElement.ToString()
                : null;

            var properties = ParseProperties(featureElement);
            mapLocationIds.UnionWith(ExtractLocationIds(properties));

            if (string.IsNullOrWhiteSpace(featureId))
            {
                continue;
            }

            featuresById[featureId] = properties;
        }
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

    private async Task SearchObservationPreviewAsync(SearchFilterInternalDto? preparedFilter = null)
    {
        if (disposed) return;
        var requestVersion = ++observationPreviewRequestVersion;
        var hasSelectedLocations = preparedFilter is null && selectedLocationIds.Count > 0;

        SearchFilterInternalDto? observationFilter;
        try
        {
            observationFilter = preparedFilter ?? JsonSerializer.Deserialize<SearchFilterInternalDto>(mapSearchFilter ?? searchFilter, jsonSerializerOptions);
            if (observationFilter is null)
            {
                Snackbar.Add("Invalid search filter.");
                isObservationPreviewLoading = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (preparedFilter is null)
            {
                ConfigureObservationPreviewFilter(
                    observationFilter,
                    hasSelectedLocations ? null : lastObservationPreviewExtent);
            }
            if (hasSelectedLocations)
            {
                observationFilter.Geographics ??= new GeographicsFilterDto();
                observationFilter.Geographics.LocationIds = ObservationPreviewLocationIds.ToArray();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Invalid search filter. {ex.Message}");
            isObservationPreviewLoading = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        isObservationPreviewLoading = true;

        try
        {
            await InvokeAsync(StateHasChanged);
            var result = await searchService.SearchObservations(
                observationFilter,
                skip: 0,
                take: ObservationPreviewTake);

            if (!disposed && requestVersion == observationPreviewRequestVersion)
            {
                observationSearchResult = result;
                await ClearObservationHighlightsAsync();
            }
        }
        catch (Exception ex)
        {
            if (!disposed && requestVersion == observationPreviewRequestVersion)
            {
                var errorPrefix = hasSelectedLocations
                    ? "Observationerna för den markerade kartfeaturen kunde inte hämtas."
                    : "Observationerna kunde inte hämtas.";
                Snackbar.Add($"{errorPrefix} {ex.Message}");
            }
        }
        finally
        {
            if (!disposed && requestVersion == observationPreviewRequestVersion)
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

        var highlight = CreateMapHighlightState();
        try
        {
            await _mapModule.InvokeVoidAsync("setHighlightedLocationIds", highlight.LocationIds, highlight.FeatureIds,
                highlight.Selection, highlight.ClickVersion);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (TaskCanceledException)
        {
        }
    }

    private sealed record MapSelectionState(int FeatureCount, IReadOnlyList<string> LocationIds, string[] FeatureIds);
    private sealed record MapHighlightState(string[] LocationIds, string[] FeatureIds, MapSelectionState Selection, int ClickVersion);

    private MapHighlightState CreateMapHighlightState()
    {
        var locationIds = ObservationPreviewLocationIds
            .Concat(highlightedObservations
            .Select(observation => observation.Location?.LocationId)
            .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
            .Select(locationId => locationId!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var featureIds = selectedFeatures
            .Where(feature => !ExtractLocationIds(feature.Properties).Any())
            .Select(feature => feature.FeatureId)
            .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
            .Select(featureId => featureId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new MapHighlightState(locationIds, featureIds,
            new MapSelectionState(selectedFeatures.Count, selectedLocationIds,
                selectedFeatures.Select(feature => feature.FeatureId).OfType<string>().Where(id => !string.IsNullOrWhiteSpace(id)).ToArray()),
            mapClickVersion);
    }

    private string ObservationPanelTitle => observationSearchResult is null
        ? "Observationer"
        : $"Observationer ({observationSearchResult.Records.Count:N0} av {observationSearchResult.TotalCount:N0})";

    private string ObservationPreviewLoadingText => selectedObservationLocation?.LocationId is not null
        ? "Hämtar observationer för vald lokal..."
        : selectedLocationIds.Count > 0 ? "Hämtar observationer för markerade kartfeatures..." : "Hämtar observationer...";

    private JsonElement ResolveClickedFeatureProperties(MapFeatureClickItem feature)
    {
        if (!string.IsNullOrWhiteSpace(feature.FeatureId) &&
            featuresById.TryGetValue(feature.FeatureId, out var knownProperties))
        {
            return knownProperties;
        }

        return feature.Properties.ValueKind == JsonValueKind.Object ? feature.Properties.Clone() : default;
    }

    private static JsonElement GetLocationArray(JsonElement properties, string name)
    {
        return properties.ValueKind == JsonValueKind.Object &&
               properties.TryGetProperty("locations", out var locations) && locations.ValueKind == JsonValueKind.Object &&
               locations.TryGetProperty(name, out var values) && values.ValueKind == JsonValueKind.Array
            ? values
            : default;
    }

    private static IEnumerable<string> ExtractLocationIds(JsonElement properties)
    {
        var ids = GetLocationArray(properties, "ids");
        if (ids.ValueKind != JsonValueKind.Array) yield break;

        foreach (var id in ids.EnumerateArray())
        {
            if (id.ValueKind == JsonValueKind.String && id.GetString() is { } value && !string.IsNullOrWhiteSpace(value))
                yield return value;
        }
    }

    private static IEnumerable<string> ExtractLocalities(JsonElement properties)
    {
        var items = GetLocationArray(properties, "items");
        if (items.ValueKind != JsonValueKind.Array) yield break;

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("locality", out var locality) && locality.ValueKind == JsonValueKind.String &&
                locality.GetString() is { } value && !string.IsNullOrWhiteSpace(value))
                yield return value;
        }
    }

    private void LoadExample(int? id)
    {
        selectedExampleId = id ?? DefaultExampleId;
        searchFilter = TaxonSearchFilterSamples.Samples[selectedExampleId.Value].Json;
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        mapSearchRequestVersion++;
        mapSearchCts?.Cancel();
        mapInitialized = false;
        observationPreviewRequestVersion++;
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

    private enum PointSizeMode
    {
        Fixed,
        ByObservationCount,
        ByObservationCountFixedBreaks
    }

    public sealed class PointSizeLegendItem
    {
        public int DiameterPx { get; set; }
        public double? MinCount { get; set; }
        public double? MaxCount { get; set; }

        public string CountText => MinCount is null ? $"≤{MaxCount:N0} observationer"
            : MaxCount is null ? $"≥{MinCount:N0} observationer"
            : MinCount == MaxCount
            ? MinCount == 1 ? "1 observation" : $"{MinCount:N0} observationer"
            : $"{MinCount:N0}–{MaxCount:N0} observationer";
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

        public JsonElement Properties { get; set; }
    }

    private sealed record SelectedMapFeature(
        string? FeatureId,
        JsonElement Properties)
    {
        public bool HasProperties => Properties.ValueKind == JsonValueKind.Object && Properties.EnumerateObject().Any();
    }

    private readonly record struct MapExtentBounds(double X1, double Y1, double X2, double Y2)
    {
        public double Width => Math.Max(0, X2 - X1);

        public double Height => Math.Max(0, Y2 - Y1);
    }
}
