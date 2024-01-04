﻿using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Gis;
using System.IO.Compression;
using System.Text;

namespace SOS.Harvest.Services
{
    public class GeoRegionApiService : Interfaces.IGeoRegionApiService
    {
        private readonly string _apiUrl;

        public GeoRegionApiService(GeoRegionApiConfiguration geoRegionApiConfiguration)
        {
            _apiUrl = geoRegionApiConfiguration.ApiUrl;
        }

        public async Task<IEnumerable<AreaDataset>?> GetAreaDatasets()
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync($"{_apiUrl}AreaDatasets");
            if (!response.IsSuccessStatusCode) throw new Exception($"Call to API failed, responseCode: {response.StatusCode}");
            var resultString = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<IEnumerable<AreaDataset>>(resultString);
        }

        public async Task<FeatureCollection?> GetFeatureCollectionFromZipAsync(IEnumerable<int> areaDatasetIds, int srid = 4326)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            var jsonBody = new StringContent(JsonConvert.SerializeObject(areaDatasetIds), Encoding.UTF8,
                "application/json");
            using var response = await client.PostAsync($"{_apiUrl}Areas/GeoJsonZip?srid={srid}", jsonBody);
            if (!response.IsSuccessStatusCode) throw new Exception($"Call to API failed, responseCode: {response.StatusCode}");

            var stream = await response.Content.ReadAsStreamAsync();
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false);
            var areasFile = zipArchive.Entries.Single(f =>
                f.Name.Equals("Areas.geojson", StringComparison.CurrentCultureIgnoreCase));
            using var areasReader = new StreamReader(areasFile.Open(), Encoding.UTF8);
            JsonReader jsonReader = new JsonTextReader(areasReader);
            var serializer = GeoJsonSerializer.CreateDefault();

            return serializer.Deserialize<FeatureCollection>(jsonReader);
        }

        public async Task<FeatureCollection?> GetFeatureCollectionWithAllAreasAsync(
            int srid = 4326)
        {
            var areaDatasets = await GetAreaDatasets();
            if (!areaDatasets?.Any() ?? true)
            {
                return null;
            }

            var featureCollection = await GetFeatureCollectionFromZipAsync(areaDatasets!.Select(m => m.Id), srid);
            return featureCollection;
        }
    }
}
