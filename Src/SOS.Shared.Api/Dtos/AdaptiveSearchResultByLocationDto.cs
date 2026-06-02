using NetTopologySuite.Geometries;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Dtos;

public class AdaptiveSearchResultByLocationDto<TObservation, TLocation>
{
    public AdaptiveSearchResultTypeDto ResultType { get; set; }
    public LatLonBoundingBoxDto BoundingBox { get; set; }
    public int TotalObservationsCount { get; set; }
    public int ObservationsCount { get; set; }
    public int ResultCount { get; set; }
    public GeoGridResultDto? GeoGridResult { get; set; }
    public IEnumerable<AdaptiveSearchLocationDto<TObservation, TLocation>>? LocationResult { get; set; }
    public IEnumerable<AreaWithGeometryDto>? FilterAreas { get; set; }

    public static AdaptiveSearchResultByLocationDto<TObservation, TLocation> CreateGeoGridResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        GeoGridResultDto geoGridResult,
        List<(Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas)
    {
        return new AdaptiveSearchResultByLocationDto<TObservation, TLocation>
        {
            ResultType = AdaptiveSearchResultTypeDto.GeoGrid,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = geoGridResult.GridCellCount,
            GeoGridResult = geoGridResult,
            FilterAreas = geographicAreas?.Select(ConvertToAreaDto).ToList()
        };
    }

    public static AdaptiveSearchResultByLocationDto<TObservation, TLocation> CreateLocationsResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        int observationsCount,
        IEnumerable<AdaptiveSearchLocationDto<TObservation, TLocation>> locations,
        List<(Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas)
    {
        return new AdaptiveSearchResultByLocationDto<TObservation, TLocation>
        {
            ResultType = AdaptiveSearchResultTypeDto.Observations,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ObservationsCount = observationsCount,
            ResultCount = locations?.Count() ?? 0,
            LocationResult = locations,
            FilterAreas = geographicAreas?.Select(ConvertToAreaDto).ToList()
        };
    }

    public static AreaWithGeometryDto ConvertToAreaDto((Lib.Models.Shared.Area area, Geometry geometry) areaWithGeometry)
    {
        var area = areaWithGeometry.area;
        var geometry = areaWithGeometry.geometry;
        return new AreaWithGeometryDto
        {
            AreaType = (AreaTypeDto)area.AreaType,
            FeatureId = area.FeatureId,
            Name = area.Name,
            BoundingBox = area.BoundingBox,
            Geometry = geometry
        };
    }
}
