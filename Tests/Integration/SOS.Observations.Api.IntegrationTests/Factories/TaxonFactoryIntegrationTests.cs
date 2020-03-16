using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Observations.Api.Database;
using SOS.Observations.Api.Factories;
using SOS.Observations.Api.Repositories;
using Xunit;
using ProcessedTaxonRepository = SOS.Observations.Api.Repositories.ProcessedTaxonRepository;

namespace SOS.Observations.Api.IntegrationTests.Factories
{
    public class TaxonFactoryIntegrationTests : TestBase
    {
        [Fact]
        public void A_taxon_tree_is_created_from_sos_processed_db()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFactory = CreateTaxonFactory();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = taxonFactory.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }

        private TaxonFactory CreateTaxonFactory()
        {
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processClient = new ProcessClient(
                mongoDbConfiguration.GetMongoDbSettings(),
                mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.BatchSize);

            var processedTaxonRepository = new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessBaseRepository<ProcessedTaxon, int>>());
            var taxonFactory = new TaxonFactory(processedTaxonRepository, new NullLogger<TaxonFactory>());
            return taxonFactory;
        }
    }
}
