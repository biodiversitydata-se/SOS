using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Extensions;

public static class AreaExtensions
{
    extension(string countyId)
    {
        public County? GetCounty()
        {
            if (string.IsNullOrEmpty(countyId)) return null;

            if (!int.TryParse(countyId, out var id))
            {
                return null;
            }

            return (County)id;
        }
    }

    extension(string municipalityId)
    {
        public Municipality? GetMunicipality()
        {
            if (string.IsNullOrEmpty(municipalityId)) return null;

            if (!int.TryParse(municipalityId, out var id))
            {
                return null;
            }

            return (Municipality)id;
        }
    }

    extension(string parishId)
    {
        public Parish? GetParish()
        {
            if (string.IsNullOrEmpty(parishId)) return null;

            if (!int.TryParse(parishId, out var id))
            {
                return null;
            }

            return (Parish)id;
        }
    }

    extension(string provinceId)
    {
        public Province? GetProvince()
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