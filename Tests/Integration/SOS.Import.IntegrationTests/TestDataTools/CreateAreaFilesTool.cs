using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.TestHelpers.IO;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateAreaFilesTool : TestBase
    {
        /// <summary>
        /// Reads field mappings from MongoDb and saves them as a JSON file.
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

            var areaVerbatimRepository = new AreaVerbatimRepository(importClient, new NullLogger<AreaVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var areas = (await areaVerbatimRepository.GetAllAsync()).ToArray();
            foreach (var tuple in filesToCreate)
            {
                GeoJsonWriter geoJsonWriter = new GeoJsonWriter();
                var featureCollection = GetFeaturecollection(areas.Where(m => m.AreaType == tuple.AreaType).ToArray());
                var strJson = geoJsonWriter.Write(featureCollection);
                var filePath = System.IO.Path.Combine(filePathBase, tuple.Filename);
                System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
            }
        }

        private FeatureCollection GetFeaturecollection(ICollection<Area> areas)
        {
            throw new NotImplementedException("Todo");
        }
    }
}