using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions.Dto;

/// <summary>Builds adaptive location-search response DTOs from server results.</summary>
public static class AdaptiveSearchByLocationResultFactory
{
    public static AdaptiveSearchResultByLocationDto<TObservation, TLocation> CreateGeoGridResult<TObservation, TLocation>(
        LatLonBoundingBoxDto? boundingBox,
        int totalObservationsCount,
        GeoGridResultDto geoGridResult,
        List<(Area area, Geometry geometry)>? geographicAreas) => new()
    {
        ResultType = AdaptiveSearchResultTypeDto.GeoGrid,
        BoundingBox = boundingBox,
        TotalObservationsCount = totalObservationsCount,
        ResultCount = geoGridResult.GridCellCount,
        GeoGridResult = geoGridResult,
        FilterAreas = geographicAreas?.Select(AdaptiveSearchResultFactory.ToAreaWithGeometryDto).ToList()
    };

    public static AdaptiveSearchResultByLocationDto<TObservation, TLocation> CreateLocationsResult<TObservation, TLocation>(
        LatLonBoundingBoxDto? boundingBox,
        int totalObservationsCount,
        int observationsCount,
        IEnumerable<AdaptiveSearchLocationDto<TObservation, TLocation>> locations,
        List<(Area area, Geometry geometry)>? geographicAreas) => new()
    {
        ResultType = AdaptiveSearchResultTypeDto.Observations,
        BoundingBox = boundingBox,
        TotalObservationsCount = totalObservationsCount,
        ObservationsCount = observationsCount,
        ResultCount = locations?.Count() ?? 0,
        LocationResult = locations,
        FilterAreas = geographicAreas?.Select(AdaptiveSearchResultFactory.ToAreaWithGeometryDto).ToList()
    };
}
