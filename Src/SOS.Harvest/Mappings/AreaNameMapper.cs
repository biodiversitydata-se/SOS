using System.Reflection;
using System.Text.Json;
using SOS.Harvest.Mappings.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Mappings
{
    /// <summary>
    ///     Loads dictionaries that can be used to merge different spelling varations
    ///     of Counties and Provinces into the intended County/Province.
    /// </summary>
    public class AreaNameMapper : IAreaNameMapper
    {
        public AreaNameMapper()
        {
            CountyNameByCountyNameSpelling = LoadCountyNameMapping();
            ProvinceNameByProvinceNameSpelling = LoadProvinceNameMapping();
        }

        public Dictionary<string, string>? CountyNameByCountyNameSpelling { get; }
        public Dictionary<string, string>? ProvinceNameByProvinceNameSpelling { get; }

        public Dictionary<string, string> BuildCountyFeatureIdByNameMapper(IEnumerable<Area> countyAreas)
        {
            var countyIdByNames = new Dictionary<string, string>();

            if (CountyNameByCountyNameSpelling == null)
            {
                return countyIdByNames;
            }

            var countyAreaByName = countyAreas.ToDictionary(x => x.Name, x => x);
            foreach (var pair in CountyNameByCountyNameSpelling)
            {
                var area = countyAreaByName[pair.Value];
                countyIdByNames.Add(pair.Key, area.FeatureId);
            }

            return countyIdByNames;
        }

        public Dictionary<string, string> BuildProvinceFeatureIdByNameMapper(IEnumerable<Area> provinceAreas)
        {
            var provinceIdByNames = new Dictionary<string, string>();
            if (ProvinceNameByProvinceNameSpelling == null)
            {
                return provinceIdByNames;
            }

            var countyAreaByName = provinceAreas.ToDictionary(x => x.Name, x => x);
            foreach (var pair in ProvinceNameByProvinceNameSpelling)
            {
                var area = countyAreaByName[pair.Value];
                provinceIdByNames.Add(pair.Key, area.FeatureId);
            }

            return provinceIdByNames;
        }

        private Dictionary<string, string>? LoadCountyNameMapping()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources/CountyNameMapper.json");
            using (var fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var countyNameMappings = JsonSerializer.DeserializeAsync<List<CountyNameMapperItem>>(fs).Result;
                return countyNameMappings?.ToDictionary(x => x.DcoCountyName ?? "", x => x.CountyName ?? "");
            }
        }

        private Dictionary<string, string>? LoadProvinceNameMapping()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources/ProvinceNameMapper.json");
            using (var fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var countyNameMappings = JsonSerializer.DeserializeAsync<List<ProvinceNameMapperItem>>(fs).Result;
                return countyNameMappings?.ToDictionary(x => x.DcoStateProvinceName ?? "", x => x.ProvinceName ?? "");
            }
        }

        private class CountyNameMapperItem
        {
            public string? DcoCountyName { get; set; }
            public string? CountyName { get; set; }
        }

        private class ProvinceNameMapperItem
        {
            public string? DcoStateProvinceName { get; set; }
            public string? ProvinceName { get; set; }
        }
    }
}