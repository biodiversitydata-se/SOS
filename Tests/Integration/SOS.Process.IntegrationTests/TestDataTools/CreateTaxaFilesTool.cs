using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Database;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.TestDataTools
{
    public class CreateTaxaFilesTool : TestBase
    {
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateListOfDarwinCoreTaxonAsMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int batchSize = 200000;
            const string filePath = @"c:\temp\AllDarwinCoreTaxa.msgpck";
            var taxonVerbatimRepository = CreateTaxonVerbatimRepository(batchSize);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxa = await taxonVerbatimRepository.GetBatchAsync(0, 0);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            byte[] bin = MessagePackSerializer.Serialize(taxa, options);
            System.IO.File.WriteAllBytes(filePath, bin);
        }


        /// <summary>
        /// Creates a Message pack file with a list of ProcessedBasicTaxon.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateListOfProcessedBasicTaxonAsMessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\AllProcessedBasicTaxa.msgpck";
            var taxonProcessedRepository = CreateTaxonProcessedRepository();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<ProcessedTaxon> taxa = await taxonProcessedRepository.GetAllAsync();
            var basicTaxa = taxa.ToProcessedBasicTaxa();
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            byte[] bin = MessagePackSerializer.Serialize(basicTaxa, options);
            System.IO.File.WriteAllBytes(filePath, bin);
        }

        private ProcessedTaxonRepository CreateTaxonProcessedRepository()
        {
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            ProcessedTaxonRepository processedTaxonRepository = new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            return processedTaxonRepository;
        }

        private TaxonVerbatimRepository CreateTaxonVerbatimRepository(int batchSize)
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);
            var taxonVerbatimRepository = new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            return taxonVerbatimRepository;
        }

    }
}