using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Process.IntegrationTests.TestDataTools
{
    public class CreateTaxaFilesTool : TestBase
    {
        private TaxonRepository CreateTaxonProcessedRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedTaxonRepository =
                new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
            return processedTaxonRepository;
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
            IEnumerable<Taxon> taxa = await taxonProcessedRepository.GetAllAsync();
            var basicTaxa = taxa.ToProcessedBasicTaxa();
            var tree = TaxonTreeFactory.CreateTaxonTree(basicTaxa.ToDictionary(t => t.Id, t => t));
            var mammaliaTaxonIds = tree.GetUnderlyingTaxonIds(MammaliaTaxonId, true);
            var mammaliaProcessedTaxa = taxa.Where(m => mammaliaTaxonIds.Contains(m.Id));
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(mammaliaProcessedTaxa, options);
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
            IEnumerable<Taxon> taxa = await taxonProcessedRepository.GetAllAsync();
            var basicTaxa = taxa.ToProcessedBasicTaxa();
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bin = MessagePackSerializer.Serialize(basicTaxa, options);
            File.WriteAllBytes(filePath, bin);
        }
    }
}