using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Database;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Repositories;
using Xunit;
using ProcessedTaxonRepository = SOS.Observations.Api.Repositories.ProcessedTaxonRepository;

namespace SOS.Observations.Api.IntegrationTests.Managers
{
    public class TaxonManagerIntegrationTests : TestBase
    {
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

        private TaxonManager CreateTaxonManager()
        {
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processClient = new ProcessClient(
                mongoDbConfiguration.GetMongoDbSettings(),
                mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.BatchSize);

            var processedTaxonRepository = new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessBaseRepository<ProcessedTaxon, int>>());
            var taxonFactory = new TaxonManager(processedTaxonRepository, new NullLogger<TaxonManager>());
            return taxonFactory;
        }
    }
}
