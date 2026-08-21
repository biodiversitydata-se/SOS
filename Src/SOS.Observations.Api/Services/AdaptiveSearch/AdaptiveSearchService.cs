using NetTopologySuite.Geometries;
using SOS.Lib.Dtos;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Gis;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services.Interfaces;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.Services.AdaptiveSearch;

internal sealed class AdaptiveSearchService : IAdaptiveSearchService
{
    private const string DefaultSortBy = "";
    private const SearchSortOrder DefaultSortOrder = SearchSortOrder.Asc;

    private readonly IObservationManager _observationManager;
    private readonly ILocationGeometryLookupService _locationGeometryLookupService;
    private readonly IAdaptiveSearchByLocationMapper _byLocationMapper;

    public AdaptiveSearchService(
        IObservationManager observationManager,
        ILocationGeometryLookupService locationGeometryLookupService,
        IAdaptiveSearchByLocationMapper byLocationMapper)
    {
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _locationGeometryLookupService = locationGeometryLookupService ?? throw new ArgumentNullException(nameof(locationGeometryLookupService));
        _byLocationMapper = byLocationMapper ?? throw new ArgumentNullException(nameof(byLocationMapper));
    }

    public async Task<AdaptiveSearchResult> SearchAsync(AdaptiveSearchRequest request)
    {
        var bbox = request.Bbox;
        var searchFilter = request.Filter.ToSearchFilterInternal(
            request.UserId,
            request.TranslationCultureCode,
            DefaultSortBy,
            DefaultSortOrder);
        var isGroupByLocation = request.OutputFormat == OutputFormatDto.GeoJsonByLocation ||
                                request.OutputFormat == OutputFormatDto.GeoJsonByLocationFlat ||
                                request.OutputFormat == OutputFormatDto.JsonByLocation;

        var outputFields = AdaptiveSearchOutputFields.EnsureCoordinates(searchFilter?.Output?.Fields);
        if (isGroupByLocation)
        {
            outputFields = AdaptiveSearchOutputFields.EnsureTaxonId(AdaptiveSearchOutputFields.EnsureLocationFields(outputFields));
        }

        if (request.CreatePolygons != CreatePolygonsMode.No)
        {
            outputFields = AdaptiveSearchOutputFields.EnsurePointPolygonFields(outputFields);
        }

        if (outputFields?.Any() ?? false)
        {
            searchFilter.Output ??= new OutputFilter();
            searchFilter.Output.Fields = outputFields;
        }

        var countAndExtentResult = await _observationManager.GetCountAndExtentAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter);

        if (!request.ReturnInputBBox || bbox == null)
        {
            bbox = countAndExtentResult.Extent;
        }

        if (countAndExtentResult.Count > request.ObservationsLimit)
        {
            return await CreateGridResultAsync(request, searchFilter, bbox, countAndExtentResult.Count);
        }

        var result = await _observationManager.GetChunkAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter,
            0,
            request.ObservationsLimit);
        if (result == null)
        {
            throw new Exception("Something went wrong when your query was executed. Make sure your filter is correct.");
        }

        var records = result.Records?.ToList() ?? new List<JsonObject>();
        Dictionary<string, Geometry>? polygonLocationGeometries = null;
        if (records.Count > 0 && (isGroupByLocation || request.CreatePolygons != CreatePolygonsMode.No))
        {
            polygonLocationGeometries = request.CreatePolygons != CreatePolygonsMode.No
                ? await _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(records)
                : null;
        }

        if (request.OutputFormat == OutputFormatDto.Json || request.OutputFormat == OutputFormatDto.JsonByLocation)
        {
            var geographicAreas = request.IncludeFilterAreasInResult
                ? await _observationManager.GetGeographicsFiltersAsAreaTuplesAsync(searchFilter)
                : null;
            object jsonResult = request.OutputFormat == OutputFormatDto.Json
                ? AdaptiveSearchResultDto<JsonObject>.CreateObservationsResult(
                    bbox.ToLatLonBoundingBoxDto(),
                    (int)countAndExtentResult.Count,
                    records,
                    geographicAreas)
                : _byLocationMapper.CreateResult(
                    bbox.ToLatLonBoundingBoxDto(),
                    (int)countAndExtentResult.Count,
                    records,
                    polygonLocationGeometries,
                    geographicAreas);

            return new AdaptiveSearchResult
            {
                Kind = AdaptiveSearchResultKind.Json,
                JsonResult = jsonResult,
                ObservationsTotalCount = countAndExtentResult.Count,
                ResultCount = GetResultCount(records, isGroupByLocation),
                ResultType = "observations"
            };
        }

        var isFlatOut = request.OutputFormat == OutputFormatDto.GeoJsonFlat ||
                        request.OutputFormat == OutputFormatDto.GeoJsonByLocationFlat;
        var geoJsonGeographicAreas = request.IncludeFilterAreasInResult
            ? await _observationManager.GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(searchFilter)
            : null;

        return new AdaptiveSearchResult
        {
            Kind = AdaptiveSearchResultKind.GeoJsonObservations,
            GeoJsonObservations = new AdaptiveSearchGeoJsonObservationsResult
            {
                Records = records,
                OutputFields = searchFilter.Output?.Fields,
                FlatOut = isFlatOut,
                PropertyLabelType = request.GeoJsonPropertyLabelType,
                ExcludeNullValues = request.ExcludeNullValues,
                Bbox = bbox,
                GeographicAreas = geoJsonGeographicAreas,
                GroupByLocation = isGroupByLocation,
                PolygonLocationGeometries = polygonLocationGeometries
            },
            ObservationsTotalCount = result.TotalCount,
            ResultCount = GetResultCount(records, isGroupByLocation),
            ResultType = "observations"
        };
    }

    private async Task<AdaptiveSearchResult> CreateGridResultAsync(
        AdaptiveSearchRequest request,
        SearchFilter searchFilter,
        SOS.Lib.Models.Gis.LatLonBoundingBox? bbox,
        long totalCount)
    {
        var zoom = GeoTileHelper.CalculateGeotileZoom(bbox, request.MaxGridCells, request.MaxZoom);
        var res = await _observationManager.GetGeogridTileAggregationAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter,
            zoom);

        if (request.OutputFormat == OutputFormatDto.Json || request.OutputFormat == OutputFormatDto.JsonByLocation)
        {
            var tileCount = bbox != null ? bbox.ToEnvelope().CalculateNumberOfTiles(zoom) : 0;
            var geogridResult = res.ToGeoGridResultDto(tileCount);
            var geographicAreas = request.IncludeFilterAreasInResult
                ? await _observationManager.GetGeographicsFiltersAsAreaTuplesAsync(searchFilter)
                : null;
            object jsonResult = request.OutputFormat == OutputFormatDto.Json
                ? AdaptiveSearchResultDto<JsonObject>.CreateGeoGridResult(
                    bbox.ToLatLonBoundingBoxDto(),
                    (int)totalCount,
                    geogridResult,
                    geographicAreas)
                : AdaptiveSearchResultByLocationDto<JsonObject, JsonObject>.CreateGeoGridResult(
                    bbox.ToLatLonBoundingBoxDto(),
                    (int)totalCount,
                    geogridResult,
                    geographicAreas);

            return new AdaptiveSearchResult
            {
                Kind = AdaptiveSearchResultKind.Json,
                JsonResult = jsonResult,
                ObservationsTotalCount = totalCount,
                ResultCount = res.GridCellTileCount,
                ResultType = "geogrid",
                Zoom = zoom
            };
        }

        var geoJsonGeographicAreas = request.IncludeFilterAreasInResult
            ? await _observationManager.GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(searchFilter)
            : null;

        return new AdaptiveSearchResult
        {
            Kind = AdaptiveSearchResultKind.GeoJsonGrid,
            GeoJson = res.GetFeatureCollectionGeoJson(CoordinateSys.WGS84, bbox, geoJsonGeographicAreas),
            ObservationsTotalCount = totalCount,
            ResultCount = res.GridCellTileCount,
            ResultType = "geogrid",
            Zoom = zoom
        };
    }

    private static int GetResultCount(IEnumerable<JsonObject> records, bool isGroupByLocation)
    {
        var recordList = records.ToList();
        if (!isGroupByLocation)
        {
            return recordList.Count;
        }

        var locationIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recordsWithoutLocationId = 0;
        foreach (var record in recordList)
        {
            var locationId = record.TryGetPropertyValue("location", out var loc) && loc is JsonObject locObj
                ? locObj["locationId"]?.GetValue<string>()
                : null;
            if (string.IsNullOrWhiteSpace(locationId))
            {
                recordsWithoutLocationId++;
            }
            else
            {
                locationIdSet.Add(locationId);
            }
        }

        return locationIdSet.Count + recordsWithoutLocationId;
    }
}
