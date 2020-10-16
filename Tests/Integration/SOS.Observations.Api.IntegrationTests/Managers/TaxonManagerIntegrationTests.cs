using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Repositories;
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

            var processedTaxonRepository = new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessBaseRepository<Taxon, int>>());
            var taxonManager = new TaxonManager(processedTaxonRepository, new NullLogger<TaxonManager>());
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