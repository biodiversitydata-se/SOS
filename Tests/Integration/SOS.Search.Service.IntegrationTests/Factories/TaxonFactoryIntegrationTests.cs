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
        public async Task Gets_basic_taxa_from_sos_processed_db()
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
            var taxa = await taxonFactory.GetBasicTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxa.Should().NotBeEmpty();
        }


        [Fact]
        public async Task Gets_taxa_from_sos_processed_db()
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
            var taxa = await taxonFactory.GetTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxa.Should().NotBeEmpty();
        }
    }
}
