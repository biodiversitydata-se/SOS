using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Utilities.Objects.Interfaces;

namespace SOS.Shared.Api.Utilities.Objects;

public class SearchFilterUtility : ISearchFilterUtility
{
    private readonly IAreaCache _areaCache;
    private readonly AreaConfiguration _areaConfiguration;
    private Envelope _swedenExtentBoundingBox;

    /// <summary>
    /// Get bounding box
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="autoAdjustBoundingBox"></param>
    /// <returns></returns>
    private async Task<LatLonBoundingBoxDto?> GetBoundingBoxAsync(
        GeographicsFilterDto filter,
        bool autoAdjustBoundingBox = true)
    {            
        Envelope swedenBoundingBox = _swedenExtentBoundingBox;
        var userBoundingBox = new Envelope([
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.BottomRight?.Latitude ?? 0)
            ]
        );
        var boundingBox = swedenBoundingBox.Intersection(userBoundingBox);

        if (autoAdjustBoundingBox)
        {
            Geometry geometryUnion = null!;

            // If areas passed, adjust bounding box to them
            if (filter?.Areas?.Any() ?? false)
            {
                var areas = await _areaCache.GetAreasAsync(filter.Areas.Select(a => ((AreaType)a.AreaType, a.FeatureId)));
                var areaGeometries = areas?.Select(a => a.BoundingBox.GetPolygon());

                var areaPolygons = areas?.Select(a => a.BoundingBox.GetPolygon());

                foreach (var areaPolygon in areaPolygons!)
                {
                    geometryUnion = geometryUnion == null ? areaPolygon : geometryUnion.Union(areaPolygon);
                }
            }

            // If geometries passed, adjust bounding box to them
            if (filter?.Geometries?.Any() ?? false)
            {
                // Try to make geometries valid and update user filter
                var validGeometries = new List<Geometry>();
                foreach(var geometry in filter?.Geometries)
                {
                    var validGeometry = geometry.TryMakeValid();
                    if (validGeometry == null)
                    {
                        filter.IsGeometryInvalid = true;
                        return null;
                    }
                    validGeometries.Add(validGeometry);

                    if (validGeometry.IsValid)
                    {
                        geometryUnion = geometryUnion == null ? geometry : geometryUnion.Union(validGeometry);
                    }
                }
                filter.Geometries = validGeometries;
            }

            if (geometryUnion != null)
            {
                boundingBox = boundingBox.AdjustByGeometry(geometryUnion, filter?.MaxDistanceFromPoint);
            }
        }

        return LatLonBoundingBoxDto.Create(boundingBox);
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
        Initialize().Wait();
    }

    private async Task Initialize()
    {
        // Get geometry of sweden economic zone
        var swedenGeometry = (await _areaCache.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1"));
        if (_areaConfiguration.SwedenExtentBufferKm.GetValueOrDefault(0) > 0)
        {
            var sweref99TmGeom = swedenGeometry.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM, false);
            sweref99TmGeom = sweref99TmGeom.Buffer(_areaConfiguration.SwedenExtentBufferKm.Value * 1000);
            swedenGeometry = sweref99TmGeom.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, false);
        }
        
        _swedenExtentBoundingBox = swedenGeometry.EnvelopeInternal;            
    }

    /// <inheritdoc/>
    public async Task<T> InitializeSearchFilterAsync<T>(T? filter) where T : SearchFilterBaseDto
    {
        filter ??= new SearchFilterBaseDto() as T;
        filter.Geographics ??= new GeographicsFilterDto();
        filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
        return filter;
    }

    /// <inheritdoc/>
    public async Task<SearchFilterInternalDto> InitializeSearchFilterAsync(SearchFilterInternalDto filter)
    {
        filter ??= new SearchFilterInternalDto();
        filter.Geographics ??= new GeographicsFilterDto();
        filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
        return filter;
    }
}
