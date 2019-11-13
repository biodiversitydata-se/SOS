using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Mappings.Interfaces
{
    public interface IAreaNameMapper
    {
        Dictionary<string, int> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas);
        Dictionary<string, int> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas);
    }
}
