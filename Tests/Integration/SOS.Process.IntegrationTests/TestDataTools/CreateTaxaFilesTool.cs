using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Database;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.TestDataTools
{
    public class CreateTaxaFilesTool : TestBase
    {
        private ProcessedTaxonRepository CreateTaxonProcessedRepository()
        {
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var processedTaxonRepository =
                new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            return processedTaxonRepository;
        }

        private TaxonVerbatimRepository CreateTaxonVerbatimRepository(int batchSize)
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);
            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            return taxonVerbatimRepository;
        }

        /// <summary>
        ///     Creates a Message pack file with a list of ProcessedBasicTaxon.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task Create_list_of_mammalia_ProcessedTaxon_as_MessagePackFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int MammaliaTaxonId = 4000107;
            const string filePath = @"c:\temp\MammaliaProcessedTaxa.msgpck";
            var taxonProcessedRepository = CreateTaxonProcessedRepository();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<ProcessedTaxon> taxa = await taxonProcessedRepository.GetAllAsync();
            var basicTaxa = taxa.ToProcessedBasicTaxa();
            var tree = TaxonTreeFactory.CreateTaxonTree(basicTaxa);
            var mammaliaTaxonIds = tree.GetUnderlyingTaxonIds(MammaliaTaxonId, true);
            var mammaliaProcessedTaxa = taxa.Where(m => mammaliaTaxonIds.Contains(m.DyntaxaTaxonId));
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(mammaliaProcessedTaxa, options);
            File.WriteAllBytes(filePath, bin);
        }

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
            var bin = MessagePackSerializer.Serialize(taxa, options);
            File.WriteAllBytes(filePath, bin);
        }


        /// <summary>
        ///     Creates a Message pack file with a list of ProcessedBasicTaxon.
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
            var bin = MessagePackSerializer.Serialize(basicTaxa, options);
            File.WriteAllBytes(filePath, bin);
        }
    }
}