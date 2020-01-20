using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.Factories;
using SOS.Import.IntegrationTests.TestHelpers.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.Verbatim.Shared;
using Xunit;

namespace SOS.Import.IntegrationTests.Taxonomy
{
    public class ImportDyntaxaDwcaIntegrationTests : TestBase
    {
        [Fact]
        public async Task Reads_a_static_dyntaxa_dwca_file_and_stores_the_taxa_in_MongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                importConfiguration.MongoDbConfiguration.DatabaseName,
                importConfiguration.MongoDbConfiguration.BatchSize);
            TaxonService taxonService = new TaxonService(
                TaxonServiceProxyStubFactory.Create(@"Resources\dyntaxa.custom.dwca.zip").Object,
                new TaxonServiceConfiguration { BaseAddress = "..." }, 
                new NullLogger<TaxonService>());

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            var taxonAttributeService = new TaxonAttributeService(
                new HttpClientService(new NullLogger<HttpClientService>()),
                new TaxonAttributeServiceConfiguration {BaseAddress = importConfiguration.TaxonAttributeServiceConfiguration.BaseAddress}, 
                new NullLogger<TaxonAttributeService>());

            var sut = new TaxonFactory(taxonVerbatimRepository, taxonService, taxonAttributeService, new NullLogger<TaxonFactory>());
        
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            HarvestInfo harvestInfo = await sut.HarvestAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Creates_a_taxon_tree_from_static_zipped_json_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var sp = Stopwatch.StartNew();
            //var taxa = DarwinCoreTaxonFactory.CreateFromFile(@"Resources\AllTaxaInMongoDb.zip");
            var taxa = DarwinCoreTaxonFactory.CreateFromFile(@"Resources\AllTaxaInMongoDbWithMinimalFields.zip");
            sp.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = TaxonTreeFactory.CreateTaxonTree<object>(taxa);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            tree.Root.ScientificName.Should().Be("Biota");
        }

        [Fact]
        public async Task Creates_a_taxon_tree_from_static_messagepack_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var sp = Stopwatch.StartNew();
            var taxa = DarwinCoreTaxonFactory.CreateFromMessagePackFile(@"Resources\AllTaxaInMongoDbWithMinimalFields.msgpck");
            //var taxa = DarwinCoreTaxonFactory.CreateFromMessagePackFile(@"Resources\AllTaxaInMongoDb.msgpck");
            sp.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = TaxonTreeFactory.CreateTaxonTree<object>(taxa);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            tree.Root.ScientificName.Should().Be("Biota");
        }
    }
}