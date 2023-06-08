using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class AreaExtensions
    {
        public static DsCounty? GetCounty(this string countyId)
        {
            if (string.IsNullOrEmpty(countyId)) return null;

            if (!int.TryParse(countyId, out var id))
            {
                return null;
            }

            return (DsCounty)id;
        }

        public static Municipality? GetMunicipality(this string municipalityId)
        {
            if (string.IsNullOrEmpty(municipalityId)) return null;

            if (!int.TryParse(municipalityId, out var id))
            {
                return null;
            }

            return (Municipality)id;
        }

        public static DsParish? GetParish(this string parishId)
        {
            if (string.IsNullOrEmpty(parishId)) return null;

            if (!int.TryParse(parishId, out var id))
            {
                return null;
            }

            return (DsParish)id;
        }

        public static DsProvince? GetProvince(this string provinceId)
        {
            if (string.IsNullOrEmpty(provinceId)) return null;

            if (!int.TryParse(provinceId, out var id))
            {
                return null;
            }

            return (DsProvince)id;
        }
    }
}
