using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Lib.Models.Search;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateTaxaFileTool : TestBase
    {
        /// <summary>
        /// Reads taxa from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateTaxaJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            // 1. Remember to first remove JsonIgnore on properties in DarwinCoreTaxon class.
            const string filePath = @"c:\temp\AllTaxa.json";
            const int batchSize = 500000; // Get all taxa
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                importConfiguration.MongoDbConfiguration.DatabaseName,
                batchSize);

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxa = await taxonVerbatimRepository.GetBatchAsync(0);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = JsonConvert.SerializeObject(taxa, serializerSettings);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }
    }
}
