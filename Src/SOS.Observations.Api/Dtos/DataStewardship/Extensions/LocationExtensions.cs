using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class LocationExtensions
    {
        public static LocationDto ToDto(this Lib.Models.Processed.Observation.Location location, CoordinateSys responseCoordinateSystem)
        {
            County? county = location?.County?.FeatureId?.GetCounty();
            Municipality? municipality = location?.Municipality?.FeatureId?.GetMunicipality();
            Parish? parish = location?.Parish?.FeatureId?.GetParish();
            Province? province = location?.Province?.FeatureId?.GetProvince();

            return new LocationDto()
            {
                County = county.Value,
                Province = province.Value,
                Municipality = municipality.Value,
                Parish = parish.Value,
                Locality = location?.Locality,
                LocationID = location?.LocationId,
                LocationRemarks = location.LocationRemarks,
                //LocationType = // ? todo - add location type to models.
                Emplacement = location?.Point.ConvertCoordinateSystem(responseCoordinateSystem)         
            };
        }
    }
}
