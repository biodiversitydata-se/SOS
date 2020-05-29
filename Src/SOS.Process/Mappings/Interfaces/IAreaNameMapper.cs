using System.Collections.Generic;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Mappings.Interfaces
{
    public interface IAreaNameMapper
    {
        Dictionary<string, string> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas);
        Dictionary<string, string> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas);
    }
}