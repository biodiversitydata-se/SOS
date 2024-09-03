﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Export.LiveIntegrationTests.TestDataTools
{
    public class CreateObservationsJsonFileTool : TestBase
    {
        /// <summary>
        ///     Reads observations from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact(Skip = "Not working")]
        [Trait("Category", "Tool")]
        public async Task CreateObservationsJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\TenProcessedTestObservations.json";
            var elasticSearchConfiguration = new ElasticSearchConfiguration();
            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedObservationRepository = new ProcessedObservationCoreRepository(
                new ElasticClientManager(elasticSearchConfiguration),
                new ElasticSearchConfiguration(),
                new ProcessedConfigurationCache(new ProcessedConfigurationRepository(exportClient, new NullLogger<ProcessedConfigurationRepository>()), new MemoryCache(new MemoryCacheOptions()), new NullLogger<ProcessedConfigurationCache>()),
                new Mock<ITaxonManager>().Object,
                new ClassCache<Dictionary<string, ClusterHealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, ClusterHealthResponse>>>()),
                new Mock<ILogger<ProcessedObservationCoreRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(new SearchFilter(0));

            var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var strJson = JsonSerializer.Serialize(observations, serializeOptions!);

            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }
    }
}