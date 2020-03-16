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
            var taxonFactory = CreateTaxonFactory(@"Resources\dyntaxa.custom.dwca.zip");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            HarvestInfo harvestInfo = await taxonFactory.HarvestAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        private TaxonFactory CreateTaxonFactory(string filename)
        {
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                importConfiguration.VerbatimDbConfiguration.BatchSize);
            TaxonService taxonService = new TaxonService(
                TaxonServiceProxyStubFactory.Create(filename).Object,
                new TaxonServiceConfiguration { BaseAddress = "..." },
                new NullLogger<TaxonService>());

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            var taxonAttributeService = new TaxonAttributeService(
                new HttpClientService(new NullLogger<HttpClientService>()),
                new TaxonAttributeServiceConfiguration { BaseAddress = importConfiguration.TaxonAttributeServiceConfiguration.BaseAddress },
                new NullLogger<TaxonAttributeService>());

            var taxonFactory = new TaxonFactory(taxonVerbatimRepository, taxonService, taxonAttributeService, new NullLogger<TaxonFactory>());
            return taxonFactory;
        }
    }
}