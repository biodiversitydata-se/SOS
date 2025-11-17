using SOS.Shared.Api.Dtos.DataStewardship.Enums;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions;

public static class AreaExtensions
{
    extension(string countyId)
    {
        public DsCounty? GetCounty()
        {
            if (string.IsNullOrEmpty(countyId)) return null;

            if (!int.TryParse(countyId, out var id))
            {
                return null;
            }

            return (DsCounty)id;
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
        public DsParish? GetParish()
        {
            if (string.IsNullOrEmpty(parishId)) return null;

            if (!int.TryParse(parishId, out var id))
            {
                return null;
            }

            return (DsParish)id;
        }
    }

    extension(string provinceId)
    {
        public DsProvince? GetProvince()
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
