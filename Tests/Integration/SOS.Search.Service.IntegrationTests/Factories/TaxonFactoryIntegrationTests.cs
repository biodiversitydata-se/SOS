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
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Repositories;
using Xunit;

namespace SOS.Search.Service.IntegrationTests.Factories
{
    public class TaxonFactoryIntegrationTests : TestBase
    {
        [Fact]
        public async Task A_taxon_tree_is_created_from_taxon_data_in_sos_processed_db()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var mongoClient = new MongoClient(mongoDbConfiguration.GetMongoDbSettings());
            var options = Options.Create(mongoDbConfiguration);
            var processedTaxonRepository = new ProcessedTaxonRepository(
                mongoClient,
                options,
                new NullLogger<BaseRepository<ProcessedTaxon, int>>());
            var taxonFactory = new TaxonFactory(processedTaxonRepository, new NullLogger<TaxonFactory>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = taxonFactory.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }
    }
}
