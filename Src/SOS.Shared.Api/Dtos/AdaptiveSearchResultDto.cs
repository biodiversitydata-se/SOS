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

    public static AdaptiveSearchResultDto<T> CreateGeoGridResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        GeoGridResultDto geoGridResult)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.GeoGrid,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = geoGridResult.GridCellCount,
            GeoGridResult = geoGridResult
        };
    }

    public static AdaptiveSearchResultDto<T> CreateObservationsResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        IEnumerable<T> observations)
    {
        return new AdaptiveSearchResultDto<T>
        {
            ResultType = AdaptiveSearchResultTypeDto.Observations,
            BoundingBox = boundingBox,
            TotalObservationsCount = totalObservationsCount,
            ResultCount = observations?.Count() ?? 0,
            ObservationsResult = observations
        };
    }
}

public enum AdaptiveSearchResultTypeDto
{
    Observations = 0,
    GeoGrid = 1
}