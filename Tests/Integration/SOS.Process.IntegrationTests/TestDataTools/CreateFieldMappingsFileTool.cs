using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Lib.Database;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Process.IntegrationTests.TestDataTools
{
    public class CreateFieldMappingsFileTool : TestBase
    {
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
            const string filePath = @"c:\temp\FieldMappings.json";
            const int batchSize = 50000;
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var verbatimClient = new ProcessClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);


            var fieldMappingRepository =
                new FieldMappingRepository(verbatimClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = await fieldMappingRepository.GetAllAsync();
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };
            var strJson = JsonConvert.SerializeObject(fieldMappings, serializerSettings);
            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateFieldMappingsMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\FieldMappings.msgpck";
            const int batchSize = 50000;
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var verbatimClient = new ProcessClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var fieldMappingRepository =
                new FieldMappingRepository(verbatimClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = await fieldMappingRepository.GetAllAsync();
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(fieldMappings, options);
            File.WriteAllBytes(filePath, bin);
        }
    }
}