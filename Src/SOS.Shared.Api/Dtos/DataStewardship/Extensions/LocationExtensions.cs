﻿using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Shared.Api.Dtos.DataStewardship.Enums;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions
{
    public static class LocationExtensions
    {
        public static DsLocationDto ToDto(this Lib.Models.Processed.Observation.Location location, CoordinateSys responseCoordinateSystem)
        {
            DsCounty? county = location?.County?.FeatureId?.GetCounty();
            Municipality? municipality = location?.Municipality?.FeatureId?.GetMunicipality();
            DsParish? parish = location?.Parish?.FeatureId?.GetParish();
            DsProvince? province = location?.Province?.FeatureId?.GetProvince();

            return new DsLocationDto()
            {
                County = county.Value,
                Province = province.Value,
                Municipality = municipality.Value,
                Parish = parish.Value,
                Locality = location?.Locality,
                LocationID = location?.LocationId,
                LocationRemarks = location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = location?.Point.Transform(CoordinateSys.WGS84, responseCoordinateSystem)
            };
        }
    }
}
