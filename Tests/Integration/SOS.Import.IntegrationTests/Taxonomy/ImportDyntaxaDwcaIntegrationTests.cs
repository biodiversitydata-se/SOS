using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.Harvesters;
using SOS.Import.IntegrationTests.TestHelpers.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Taxonomy
{
    public class ImportDyntaxaDwcaIntegrationTests : TestBase
    {
        private TaxonHarvester CreateTaxonHarvester(string filename)
        {
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                importConfiguration.VerbatimDbConfiguration.BatchSize);
            var taxonService = new TaxonService(
                TaxonServiceProxyStubFactory.Create(filename).Object,
                new TaxonServiceConfiguration {BaseAddress = "..."},
                new NullLogger<TaxonService>());

            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(importClient, new NullLogger<TaxonVerbatimRepository>());

            var taxonAttributeService = new TaxonAttributeService(
                new HttpClientService(new NullLogger<HttpClientService>()),
                new TaxonAttributeServiceConfiguration
                    {BaseAddress = importConfiguration.TaxonAttributeServiceConfiguration.BaseAddress},
                new NullLogger<TaxonAttributeService>());

            var taxonHarvester = new TaxonHarvester(taxonVerbatimRepository, taxonService, taxonAttributeService,
                new NullLogger<TaxonHarvester>());
            return taxonHarvester;
        }

        [Fact]
        public async Task Reads_a_static_dyntaxa_dwca_file_and_stores_the_taxa_in_MongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonHarvester = CreateTaxonHarvester(@"Resources\dyntaxa.custom.dwca.zip");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await taxonHarvester.HarvestAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }
    }
}