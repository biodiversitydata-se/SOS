using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Export.Repositories;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Search.Service.Database;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Repositories;
using Xunit;
using ProcessedTaxonRepository = SOS.Search.Service.Repositories.ProcessedTaxonRepository;

namespace SOS.Search.Service.IntegrationTests.Factories
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
