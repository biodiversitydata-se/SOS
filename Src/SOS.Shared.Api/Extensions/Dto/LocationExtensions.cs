using NetTopologySuite.Geometries;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Location;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Shared.Api.Extensions.Dto;

public static class LocationExtensions
{
    /// <summary>
    /// Cast Location to locationDto
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static LocationDto ToDto(this Location location)
    {
        if (location == null)
        {
            return null!;
        }

        return new LocationDto
        {
            Continent = location.Continent == null
                ? null!
                : new IdValueDto<int> { Id = location.Continent.Id, Value = location.Continent.Value },
            CoordinatePrecision = location.CoordinatePrecision,
            CoordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters,
            Country = location.Country == null
                ? null!
                : new IdValueDto<int> { Id = location.Country.Id, Value = location.Country.Value },
            CountryCode = location.CountryCode,
            CountryRegion = location.CountryRegion == null
                ? null!
                : new IdValueDto<string> { Id = location.CountryRegion.FeatureId, Value = location.CountryRegion.Name },
            County = location.County == null
                ? null!
                : new IdValueDto<string> { Id = location.County.FeatureId, Value = location.County.Name },
            DecimalLatitude = location.DecimalLatitude,
            DecimalLongitude = location.DecimalLongitude,
            ExternalId = location.Attributes?.ExternalId!,
            Locality = location.Locality,
            LocationAccordingTo = location.LocationAccordingTo,
            LocationId = location.LocationId,
            LocationRemarks = location.LocationRemarks,
            Municipality = location.Municipality == null
                ? null!
                : new IdValueDto<string> { Id = location.Municipality.FeatureId, Value = location.Municipality.Name },
            Province = location.Province == null
                ? null!
                : new IdValueDto<string> { Id = location.Province.FeatureId, Value = location.Province.Name },
            Parish = location.Parish == null
                ? null!
                : new IdValueDto<string> { Id = location.Parish.FeatureId, Value = location.Parish.Name },
            Point = location.Point,
            PointWithBuffer = location.PointWithBuffer,
            PointWithDisturbanceBuffer = location.PointWithDisturbanceBuffer,
            ProjectId = location.Attributes?.ProjectId,
            Type = location.Type
        };
    }

    public static LocationSearchResultDto ToDto(this LocationSearchResult locationSearchResult)
    {
        if (locationSearchResult == null)
        {
            return null!;
        }

        return new LocationSearchResultDto
        {
            County = locationSearchResult.County,
            Id = locationSearchResult.Id,
            Latitude = locationSearchResult.Latitude,
            Longitude = locationSearchResult.Longitude,
            Municipality = locationSearchResult.Municipality,
            Name = locationSearchResult.Name,
            Parish = locationSearchResult.Parish
        };
    }
}