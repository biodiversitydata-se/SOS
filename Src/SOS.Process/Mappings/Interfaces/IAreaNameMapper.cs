using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Mappings.Interfaces
{
    public interface IAreaNameMapper
    {
        Dictionary<string, string> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas);
        Dictionary<string, string> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas);
    }
}
