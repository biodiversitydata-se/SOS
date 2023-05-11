using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class AreaExtensions
    {
        public static County? GetCounty(this string countyId)
        {
            if (string.IsNullOrEmpty(countyId)) return null;

            if (!int.TryParse(countyId, out var id))
            {
                return null;
            }

            return (County)id;
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

        public static Parish? GetParish(this string parishId)
        {
            if (string.IsNullOrEmpty(parishId)) return null;

            if (!int.TryParse(parishId, out var id))
            {
                return null;
            }

            return (Parish)id;
        }

        public static Province? GetProvince(this string provinceId)
        {
            if (string.IsNullOrEmpty(provinceId)) return null;

            if (!int.TryParse(provinceId, out var id))
            {
                return null;
            }

            return (Province)id;
        }
    }
}
