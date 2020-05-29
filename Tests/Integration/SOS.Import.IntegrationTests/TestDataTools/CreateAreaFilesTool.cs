using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateAreaFilesTool : TestBase
    {
        private FeatureCollection GetFeaturecollection(ICollection<Area> areas)
        {
            throw new NotImplementedException("Todo");
        }

        /// <summary>
        ///     Reads field mappings from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateFieldMappingsJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var filesToCreate = new[]
            {
                (AreaType: AreaType.County, Filename: "Counties.geojson"),
                (AreaType: AreaType.Province, Filename: "Provinces.geojson")
            };
            const string filePathBase = @"c:\temp\";
            const int batchSize = 50000;
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);

            var areaVerbatimRepository =
                new AreaVerbatimRepository(importClient, new NullLogger<AreaVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var areas = (await areaVerbatimRepository.GetAllAsync()).ToArray();
            foreach (var tuple in filesToCreate)
            {
                var geoJsonWriter = new GeoJsonWriter();
                var featureCollection = GetFeaturecollection(areas.Where(m => m.AreaType == tuple.AreaType).ToArray());
                var strJson = geoJsonWriter.Write(featureCollection);
                var filePath = Path.Combine(filePathBase, tuple.Filename);
                File.WriteAllText(filePath, strJson, Encoding.UTF8);
            }
        }
    }
}