using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers;
using SOS.Process.Mappings.Interfaces;

namespace SOS.Process.Mappings
{
    /// <summary>
    /// Loads dictionaries that can be used to merge different spelling varations
    /// of Counties and Provinces into the intended County/Province.
    /// </summary>
    public class AreaNameMapper : IAreaNameMapper
    {
        public Dictionary<string, string> CountyNameByCountyNameSpelling { get; }
        public Dictionary<string, string> ProvinceNameByProvinceNameSpelling { get; }

        public Dictionary<string, int> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas)
        {
            Dictionary<string, int> countyIdByNames = new Dictionary<string, int>();
            var countyAreaByName = countyAreas.ToDictionary(x => x.Name, x => x);
            foreach (var pair in CountyNameByCountyNameSpelling)
            {
                var area = countyAreaByName[pair.Value];
                countyIdByNames.Add(pair.Key, area.FeatureId);
            }

            return countyIdByNames;
        }

        public Dictionary<string, int> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas)
        {
            Dictionary<string, int> provinceIdByNames = new Dictionary<string, int>();
            var countyAreaByName = provinceAreas.ToDictionary(x => x.Name, x => x);
            foreach (var pair in ProvinceNameByProvinceNameSpelling)
            {
                var area = countyAreaByName[pair.Value];
                provinceIdByNames.Add(pair.Key, area.FeatureId);
            }

            return provinceIdByNames;
        }


        public AreaNameMapper()
        {
            CountyNameByCountyNameSpelling = LoadCountyNameMapping();
            ProvinceNameByProvinceNameSpelling = LoadProvinceNameMapping();
        }

        private Dictionary<string, string> LoadCountyNameMapping()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\CountyNameMapper.json");
            using (FileStream fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var countyNameMappings = JsonSerializer.DeserializeAsync<List<CountyNameMapperItem>>(fs).Result;
                return countyNameMappings.ToDictionary(x => x.DcoCountyName, x => x.CountyName);
            }
        }

        private Dictionary<string, string> LoadProvinceNameMapping()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\ProvinceNameMapper.json");
            using (FileStream fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var countyNameMappings = JsonSerializer.DeserializeAsync<List<ProvinceNameMapperItem>>(fs).Result;
                return countyNameMappings.ToDictionary(x => x.DcoStateProvinceName, x => x.ProvinceName);
            }
        }

        private class CountyNameMapperItem
        {
            public string DcoCountyName { get; set; }
            public string CountyName { get; set; }
        }

        private class ProvinceNameMapperItem
        {
            public string DcoStateProvinceName { get; set; }
            public string ProvinceName { get; set; }
        }
    }
}
