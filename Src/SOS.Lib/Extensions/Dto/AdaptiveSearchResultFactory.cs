using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Lib.Extensions.Dto;

/// <summary>
/// Builds public adaptive-search contracts from the server's search results and geometries.
/// </summary>
public static class AdaptiveSearchResultFactory
{
    public static AdaptiveSearchResultDto<T> CreateGeoGridResult<T>(
        LatLonBoundingBoxDto? boundingBox,
        int totalObservationsCount,
        GeoGridResultDto geoGridResult,
        List<(SOS.Lib.Models.Shared.Area area, NetTopologySuite.Geometries.Geometry geometry)>? geographicAreas)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.GeoGrid,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = geoGridResult.GridCellCount,
            GeoGridResult = geoGridResult,
            FilterAreas = geographicAreas?.Select(ToAreaWithGeometryDto).ToList()
        };
    }

    public static AdaptiveSearchResultDto<T> CreateObservationsResult<T>(
        LatLonBoundingBoxDto? boundingBox,
        int totalObservationsCount,
        IEnumerable<T>? observations,
        List<(SOS.Lib.Models.Shared.Area area, NetTopologySuite.Geometries.Geometry geometry)>? geographicAreas)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.Observations,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = observations?.Count() ?? 0,
            ObservationsResult = observations,
            FilterAreas = geographicAreas?.Select(ToAreaWithGeometryDto).ToList()
        };
    }

    public static AreaWithGeometryDto ToAreaWithGeometryDto((SOS.Lib.Models.Shared.Area area, NetTopologySuite.Geometries.Geometry geometry) source)
    {
        return new AreaWithGeometryDto
        {
            AreaType = (AreaTypeDto)source.area.AreaType,
            FeatureId = source.area.FeatureId,
            Name = source.area.Name,
            BoundingBox = source.area.BoundingBox.GetCompleteBoundingBox()!,
            Geometry = source.geometry?.ToGeoJsonGeometry()
        };
    }
}
