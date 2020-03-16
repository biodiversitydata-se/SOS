using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Factories;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Lib.Models.Search;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Export.IntegrationTests.TestDataTools
{
    public class CreateObservationsJsonFileTool : TestBase
    {
        /// <summary>
        /// Reads observations from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category","Tool")]
        public async Task CreateObservationsJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservations = 10;
            const string filePath = @"c:\temp\TenProcessedTestObservations.json";
            var exportConfiguration = GetExportConfiguration();
            var exportClient = new ExportClient(
                exportConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.ProcessedDbConfiguration.DatabaseName,
                exportConfiguration.ProcessedDbConfiguration.BatchSize);
            var processedObservationRepository = new ProcessedObservationRepository(
                exportClient,
                new TaxonFactory(
                    new ProcessedTaxonRepository(exportClient, new Mock<ILogger<ProcessedTaxonRepository>>().Object), new Mock<ILogger<TaxonFactory>>().Object),
                new Mock<ILogger<ProcessedObservationRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await processedObservationRepository.GetChunkAsync(new SearchFilter(), 0,nrObservations);

            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = JsonConvert.SerializeObject(observations, serializerSettings);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

    }
}
