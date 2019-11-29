using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Test.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Export.Test.Tools
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
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                new ExportClient(
                    exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    exportConfiguration.MongoDbConfiguration.DatabaseName,
                    exportConfiguration.MongoDbConfiguration.BatchSize),
                new Mock<ILogger<ProcessedDarwinCoreRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await processedDarwinCoreRepository.GetChunkAsync(0, nrObservations);


            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = JsonConvert.SerializeObject(observations, serializerSettings);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

    }
}
