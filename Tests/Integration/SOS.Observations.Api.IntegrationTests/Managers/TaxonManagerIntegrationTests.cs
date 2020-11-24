using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.Repositories.Resource;
using SOS.Observations.Api.Managers;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.Managers
{
    public class TaxonManagerIntegrationTests : TestBase
    {
        private TaxonManager CreateTaxonManager()
        {
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processClient = new ProcessClient(
                mongoDbConfiguration.GetMongoDbSettings(),
                mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize,
                mongoDbConfiguration.WriteBatchSize);

            var processedTaxonRepository = new TaxonRepository(
                processClient,
                new NullLogger<TaxonRepository>());
            var taxonManager = new TaxonManager(processedTaxonRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<TaxonManager>());
            return taxonManager;
        }

        [Fact]
        public void A_taxon_tree_is_created_from_sos_processed_db()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonManager = CreateTaxonManager();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = taxonManager.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }
    }
}