using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Lib.Database;
using SOS.Lib.Extensions;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateTaxaFileTool : TestBase
    {
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateBasicTaxaMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            // 1. Remember to first remove JsonIgnore on properties in DarwinCoreTaxon class.
            const string filePath = @"c:\temp\AllBasicTaxa.msgpck";
            const int batchSize = 500000; // Get all taxa
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxa = await taxonVerbatimRepository.GetBatchAsync(0);
            var basicTaxa = taxa.Select(m => m.ToProcessedBasicTaxon());
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(basicTaxa, options);
            File.WriteAllBytes(filePath, bin);
        }

        /// <summary>
        ///     Reads taxa from MongoDb and saves them as a JSON file.
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
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxa = await taxonVerbatimRepository.GetBatchAsync(0);
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };
            var strJson = JsonConvert.SerializeObject(taxa, serializerSettings);
            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }

        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateTaxaMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            // 1. Remember to first remove JsonIgnore on properties in DarwinCoreTaxon class.
            const string filePath = @"c:\temp\AllTaxa.msgpck";
            const int batchSize = 500000; // Get all taxa
            var importConfiguration = GetImportConfiguration();
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxa = await taxonVerbatimRepository.GetBatchAsync(0);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(taxa, options);
            File.WriteAllBytes(filePath, bin);
        }
    }
}