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
        public async Task Reads_a_static_dyntaxa_dwca_file_and_verifies_multiple_taxon_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            TaxonServiceConfiguration taxonServiceConfiguration = new TaxonServiceConfiguration() { BaseAddress = "..." };
            var taxonServiceProxyStub = TaxonServiceProxyStubFactory.Create(@"Resources\dyntaxa.custom.dwca.zip");
            var sut = new TaxonService(taxonServiceProxyStub.Object, taxonServiceConfiguration, new NullLogger<TaxonService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<DarwinCoreTaxon> taxa = await sut.GetTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech species DarwinCore properties
            //-----------------------------------------------------------------------------------------------------------
            //var beechSpeciesTaxon = taxa.Single(t => t.TaxonID == "urn:lsid:dyntaxa.se:Taxon:220778");
            var beechSpeciesTaxon = taxa.Single(t => t.Id == 220778);
            beechSpeciesTaxon.TaxonRank.Should().Be("species");
            beechSpeciesTaxon.Kingdom.Should().Be("Plantae");
            beechSpeciesTaxon.ScientificName.Should().Be("Fagus sylvatica");
            beechSpeciesTaxon.ScientificNameAuthorship.Should().Be("L.");
            beechSpeciesTaxon.VernacularName.Should().Be("bok");
            beechSpeciesTaxon.NomenclaturalStatus.Should().Be("valid");
            beechSpeciesTaxon.TaxonomicStatus.Should().Be("accepted");
            beechSpeciesTaxon.DynamicProperties.DyntaxaTaxonId.Should().Be(220778);
            beechSpeciesTaxon.HigherClassification.Should().Be("Biota | Plantae | Viridiplantae | Streptophyta | Embryophyta | Tracheophyta | Euphyllophytina | Spermatophytae | Angiospermae | Magnoliopsida | Fagales | Fagaceae | Fagus");

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech (bok) species has one recommended swedish vernacular name,
            //          two swedish synonyms and one english synonym
            //-----------------------------------------------------------------------------------------------------------
            var expectedBeechSpeciesTaxonVernacularNames = CreateExpectedBeechSpeciesTaxonVernacularNames();
            beechSpeciesTaxon.DynamicProperties.VernacularNames.Should().BeEquivalentTo(expectedBeechSpeciesTaxonVernacularNames);

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech (bok) genus has one main parent and one secondary parent.
            //-----------------------------------------------------------------------------------------------------------
            var beechGenusTaxon = taxa.Single(t => t.TaxonID == "urn:lsid:dyntaxa.se:Taxon:1006037");
            beechGenusTaxon.DynamicProperties.ParentDyntaxaTaxonId.Should().Be(2002757); // Family - Fagaceae (bokväxter)
            beechGenusTaxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds.Should().BeEquivalentTo(new List<int> { 6005257 }); // Organism group - Hardwood forest trees (ädellövträd)
        }

        [Fact]
        public async Task Creates_a_taxon_tree_and_verifies_that_Ichthyaetus_genus_has_5_underlying_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            
            TaxonServiceConfiguration taxonServiceConfiguration = new TaxonServiceConfiguration() { BaseAddress = "..." };
            var taxonServiceProxyStub = TaxonServiceProxyStubFactory.Create(@"Resources\dyntaxa.custom.dwca.zip");
            var taxonService = new TaxonService(taxonServiceProxyStub.Object, taxonServiceConfiguration, new NullLogger<TaxonService>());
            IEnumerable<DarwinCoreTaxon> taxa = await taxonService.GetTaxaAsync();
            var tree = TaxonTreeFactory.CreateTaxonTree<object>(taxa);
            const int ichthyaetusTaxonId = 6011885;
            int[] expectedUnderlyingTaxonIds = { 266836, 103067, 266238, 267106, 266835 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var underlyingTaxa = tree.GetUnderlyingTaxonIds(ichthyaetusTaxonId, false);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            underlyingTaxa.Should().HaveCount(5);
            underlyingTaxa.Should().BeEquivalentTo(expectedUnderlyingTaxonIds);
        }

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


        private List<TaxonVernacularName> CreateExpectedBeechSpeciesTaxonVernacularNames()
        {
            List<TaxonVernacularName> beechVernacularNames = new List<TaxonVernacularName>
            {
                new TaxonVernacularName { Name = "bok", Language = "sv", CountryCode = "SE", IsPreferredName = true },
                new TaxonVernacularName { Name = "vanlig bok", Language = "sv", CountryCode = "SE", IsPreferredName = false },
                new TaxonVernacularName { Name = "rödbok", Language = "sv", CountryCode = "SE", IsPreferredName = false },
                new TaxonVernacularName { Name = "Beech", Language = "en", CountryCode = "GB", IsPreferredName = false }
            };

            return beechVernacularNames;
        }
    }
}
