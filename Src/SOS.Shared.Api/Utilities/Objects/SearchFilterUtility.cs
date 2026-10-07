using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Search.Input;

namespace SOS.Shared.Api.Utilities.Objects;

public class SearchFilterUtility : ISearchFilterUtility
{
    private readonly IAreaCache _areaCache;
    private readonly AreaConfiguration _areaConfiguration;
    private Envelope _zoneOfSosBoundingBox;

    /// <summary>
    /// Get bounding box
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="geometries">Server geometries, converted once when preparing the filter.</param>
    /// <param name="autoAdjustBoundingBox"></param>
    /// <returns></returns>
    private async Task<(LatLonBoundingBoxDto? BoundingBox, IReadOnlyCollection<Geometry>? Geometries)> GetBoundingBoxAsync(
        GeographicsFilterDto filter,
        IReadOnlyCollection<Geometry>? geometries,
        bool autoAdjustBoundingBox = true)
    {
        if (filter.HasGeometryConversionError())
        {
            return (null, geometries);
        }

        Envelope zoneOfSosBoundingBox = _zoneOfSosBoundingBox;
        var userBoundingBox = new Envelope([
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.BottomRight?.Latitude ?? 0)
            ]
        );
        var boundingBox = zoneOfSosBoundingBox.Intersection(userBoundingBox);

        if (autoAdjustBoundingBox)
        {
            Geometry geometryUnion = null!;

            // If areas passed, adjust bounding box to them
            if (filter?.Areas?.Any() ?? false)
            {
                var areas = await _areaCache.GetAreasAsync(filter.Areas.Select(a => ((AreaType)a.AreaType, a.FeatureId)));
                var areaPolygons = areas?.Select(a => a.BoundingBox.GetPolygon());

                foreach (var areaPolygon in areaPolygons!)
                {
                    geometryUnion = geometryUnion == null ? areaPolygon : geometryUnion.Union(areaPolygon);
                }
            }

            // If geometries passed, adjust bounding box to them
            if (geometries?.Any() ?? false)
            {
                // Keep the normalized geometries for validation and search.
                var validGeometries = new List<Geometry>();
                foreach(var geometry in geometries!)
                {
                    var validGeometry = geometry.TryMakeValid();
                    if (validGeometry == null)
                    {
                        return (null, geometries);
                    }
                    validGeometries.Add(validGeometry);

                    if (validGeometry.IsValid)
                    {
                        geometryUnion = geometryUnion == null ? geometry : geometryUnion.Union(validGeometry);
                    }
                }
                geometries = validGeometries;
            }

            if (geometryUnion != null)
            {
                boundingBox = boundingBox.AdjustByGeometry(geometryUnion, filter?.MaxDistanceFromPoint);
            }
        }

        return (new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto
            {
                Latitude = boundingBox.MaxY,
                Longitude = boundingBox.MinX
            },
            BottomRight = new LatLonCoordinateDto
            {
                Latitude = boundingBox.MinY,
                Longitude = boundingBox.MaxX
            }
        }, geometries);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="areaConfiguration"></param>
    /// <param name="areaCache"></param>
    public SearchFilterUtility(
        AreaConfiguration areaConfiguration,
        IAreaCache areaCache
    )
    {
        _areaConfiguration = areaConfiguration ?? throw new ArgumentNullException(nameof(areaConfiguration));
        _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        InitializeAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        // Get geometry of sweden economic zone
        var swedenGeometry = (await _areaCache.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1"));
        if (_areaConfiguration.SwedenExtentBufferKm.GetValueOrDefault(0) > 0)
        {
            var sweref99TmGeom = swedenGeometry.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM, false);
            sweref99TmGeom = sweref99TmGeom.Buffer(_areaConfiguration.SwedenExtentBufferKm.Value * 1000);
            swedenGeometry = sweref99TmGeom.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, false);
        }
        
        _zoneOfSosBoundingBox = swedenGeometry.EnvelopeInternal;            
    }

    public Task<SearchFilterBaseDto> InitializeSearchFilterAsync(SearchFilterBaseDto? filter) =>
        InitializeSearchFilterAsync<SearchFilterBaseDto>(filter ?? new SearchFilterBaseDto());

    public Task<SearchFilterDto> InitializeSearchFilterAsync(SearchFilterDto? filter) =>
        InitializeSearchFilterAsync<SearchFilterDto>(filter ?? new SearchFilterDto());

    public Task<SearchFilterInternalBaseDto> InitializeSearchFilterAsync(SearchFilterInternalBaseDto? filter) =>
        InitializeSearchFilterAsync<SearchFilterInternalBaseDto>(filter ?? new SearchFilterInternalBaseDto());

    public Task<SearchFilterInternalDto> InitializeSearchFilterAsync(SearchFilterInternalDto? filter) =>
        InitializeSearchFilterAsync<SearchFilterInternalDto>(filter ?? new SearchFilterInternalDto());

    public Task<SearchFilterAggregationInternalDto> InitializeSearchFilterAsync(SearchFilterAggregationInternalDto? filter) =>
        InitializeSearchFilterAsync<SearchFilterAggregationInternalDto>(filter ?? new SearchFilterAggregationInternalDto());

    /// <inheritdoc/>
    public async Task<T> InitializeSearchFilterAsync<T>(T? filter) where T : SearchFilterBaseDto
    {
        if (filter is null) throw new ArgumentNullException(nameof(filter));
        var input = GeographicsFilterInputModel.FromDto(filter.Geographics);
        filter.Geographics = input;
        var prepared = await GetBoundingBoxAsync(input, input.ServerGeometries);
        input.BoundingBox = prepared.BoundingBox;
        input.ServerGeometries = prepared.Geometries as List<Geometry> ?? prepared.Geometries?.ToList();
        return filter;
    }
}
