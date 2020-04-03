using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Newtonsoft.Json;
using SOS.Export.Managers;
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
            var elasticClient = new ElasticClient();
            var exportClient = new ExportClient(
                exportConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.ProcessedDbConfiguration.DatabaseName,
                exportConfiguration.ProcessedDbConfiguration.BatchSize);
            var processedObservationRepository = new ProcessedObservationRepository(
                elasticClient,
                exportClient,
                new Mock<ILogger<ProcessedObservationRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var chunkResult = await processedObservationRepository.StartGetChunkAsync(new SearchFilter(), 0, nrObservations);
            var observations = chunkResult.Documents;            

            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = JsonConvert.SerializeObject(observations, serializerSettings);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

    }
}
