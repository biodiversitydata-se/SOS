using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Lib.Extensions;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateFieldMappingsFileTool : TestBase
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
            const string filePath = @"c:\temp\FieldMappings.json";
            const int batchSize = 50000; 
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);

            var fieldMappingRepository =
                new FieldMappingRepository(importClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = await fieldMappingRepository.GetAllAsync();
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = JsonConvert.SerializeObject(fieldMappings, serializerSettings);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
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
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);

            var fieldMappingRepository =
                new FieldMappingRepository(importClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = await fieldMappingRepository.GetAllAsync();
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            byte[] bin = MessagePackSerializer.Serialize(fieldMappings, options);
            System.IO.File.WriteAllBytes(filePath, bin);
        }
    }
}
