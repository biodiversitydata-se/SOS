﻿using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Locations.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto.DataStewardship;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions.Dto;

public static class LocationExtensions
{
    extension(Location location)
    {
        /// <summary>
        /// Maps a location to its API response DTO, converting its geometries to GeoJSON.
        /// </summary>
        /// <param name="includeGeometries">False when the server writes the original geometries directly to JSON.</param>
        public LocationDto ToLocationDto(bool includeGeometries = true)
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
                Point = includeGeometries ? location.Point.ToGeoJsonPoint() : null,
                PointWithBuffer = includeGeometries ? location.PointWithBuffer?.ToGeoJsonGeometry() : null,
                PointWithDisturbanceBuffer = includeGeometries ? location.PointWithDisturbanceBuffer?.ToGeoJsonGeometry() : null,
                ProjectId = location.Attributes?.ProjectId,
                Type = (SOS.Api.Contracts.Observations.Responses.LocationType?)location.Type
            };
        }

        public DsLocationDto ToDsLocationDto(CoordinateSys responseCoordinateSystem)
        {
            if (location == null)
            {
                return null!;
            }

            return new DsLocationDto()
            {
                County = location.County?.FeatureId?.GetCounty(),
                Province = location.Province?.FeatureId?.GetProvince(),
                Municipality = location.Municipality?.FeatureId?.GetMunicipality(),
                Parish = location.Parish?.FeatureId?.GetParish(),
                Locality = location.Locality,
                LocationID = location.LocationId,
                LocationRemarks = location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = location.Point.Transform(CoordinateSys.WGS84, responseCoordinateSystem).ToGeoJsonPoint()
            };
        }
    }
}
