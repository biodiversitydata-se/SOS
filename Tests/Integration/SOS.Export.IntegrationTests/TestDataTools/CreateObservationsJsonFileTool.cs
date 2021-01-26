using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Export.IntegrationTests.TestDataTools
{
    public class CreateObservationsJsonFileTool : TestBase
    {
        /// <summary>
        ///     Reads observations from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateObservationsJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\TenProcessedTestObservations.json";
            var elasticClient = new ElasticClient();
            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedObservationRepository = new ProcessedPublicObservationRepository(
                exportClient,
                elasticClient,
                new ElasticSearchConfiguration(),
                new Mock<ILogger<ProcessedPublicObservationRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await processedObservationRepository.ScrollObservationsAsync(new SearchFilter(), null);

            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };
            var strJson = JsonConvert.SerializeObject(observations, serializerSettings);
            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }
    }
}