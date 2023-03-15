using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.JsonConverters;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateVocabularyFileTool : TestBase
    {
        /// <summary>
        ///     Reads vocabularies from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateVocabulariesJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\Vocabularies.json";
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient (
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processedVocabularies = await vocabularyRepository.GetAllAsync();
            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true, };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var strJson = JsonSerializer.Serialize(processedVocabularies, serializeOptions);

            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateVocabulariesMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\Vocabularies.msgpck";
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var importClient = new ProcessClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var vocabularyRepository =
                new VocabularyRepository(importClient, new NullLogger<VocabularyRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processedVocabularies = await vocabularyRepository.GetAllAsync();
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(processedVocabularies, options);
            File.WriteAllBytes(filePath, bin);
        }
    }
}