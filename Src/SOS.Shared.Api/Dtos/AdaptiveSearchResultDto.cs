using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Dtos;

public class AdaptiveSearchResultDto<T>
{
    public AdaptiveSearchResultTypeDto ResultType { get; set; }
    public LatLonBoundingBoxDto BoundingBox { get; set; }
    public int TotalObservationsCount { get; set; }
    public int ResultCount { get; set; }
    public GeoGridResultDto? GeoGridResult { get; set; }    
    public IEnumerable<T>? ObservationsResult { get; set; }
    public IEnumerable<AreaWithGeometryDto>? FilterAreas { get; set; }

    public static AdaptiveSearchResultDto<T> CreateGeoGridResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        GeoGridResultDto geoGridResult,
        List<(Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.GeoGrid,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = geoGridResult.GridCellCount,
            GeoGridResult = geoGridResult,
            FilterAreas = geographicAreas?.Select(ConvertToAreaDto).ToList()
        };
    }

    public static AdaptiveSearchResultDto<T> CreateObservationsResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        IEnumerable<T> observations,
        List<(Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.Observations,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = observations?.Count() ?? 0,
            ObservationsResult = observations,
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

public enum AdaptiveSearchResultTypeDto
{
    Observations = 0,
    GeoGrid = 1
}