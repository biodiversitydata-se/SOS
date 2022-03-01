using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Mappings.Interfaces
{
    public interface IAreaNameMapper
    {
        Dictionary<string, string> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas);
        Dictionary<string, string> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas);
    }
}