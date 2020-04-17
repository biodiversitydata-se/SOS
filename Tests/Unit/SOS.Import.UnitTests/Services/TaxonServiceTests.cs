using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Import.UnitTests.TestHelpers.Factories;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.DarwinCore;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class TaxonServiceTests
    {
        private readonly Mock<ITaxonServiceProxy> _taxonServiceProxyMock;
        private readonly TaxonServiceConfiguration _taxonServiceConfiguration;
        private readonly Mock<ILogger<TaxonService>> _loggerMock;

        private TaxonService TestObject => new TaxonService(
            _taxonServiceProxyMock.Object,
            _taxonServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonServiceTests()
        {
            _taxonServiceConfiguration = new TaxonServiceConfiguration { BaseAddress = "https://taxonservice.artdata.slu.se/DarwinCore/DarwinCoreArchiveFile" };
            _loggerMock = new Mock<ILogger<TaxonService>>();
            _taxonServiceProxyMock = new Mock<ITaxonServiceProxy>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonService(
                _taxonServiceProxyMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonServiceConfiguration");

            create = () => new TaxonService(
                _taxonServiceProxyMock.Object, 
                _taxonServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Get taxa success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxaAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetTaxaAsync();
          
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Get taxa fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxaAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        [Fact]
        public async Task Parse_a_static_dyntaxa_dwca_file_and_verifies_multiple_taxon_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            TaxonServiceConfiguration taxonServiceConfiguration = new TaxonServiceConfiguration { BaseAddress = "..." };
            var taxonServiceProxyStub = TaxonServiceProxyStubFactory.Create(@"Resources\dyntaxa.custom.dwca.zip");
            var sut = new TaxonService(taxonServiceProxyStub.Object, taxonServiceConfiguration, new NullLogger<TaxonService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<DarwinCoreTaxon> taxa = await sut.GetTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech species DarwinCore properties
            //-----------------------------------------------------------------------------------------------------------
            var beechSpeciesTaxon = taxa.Single(t => t.Id == 220778);
            beechSpeciesTaxon.TaxonRank.Should().Be("species");
            beechSpeciesTaxon.Kingdom.Should().Be("Plantae");
            beechSpeciesTaxon.ScientificName.Should().Be("Fagus sylvatica");
            beechSpeciesTaxon.ScientificNameAuthorship.Should().Be("L.");
            beechSpeciesTaxon.VernacularName.Should().Be("bok");
            beechSpeciesTaxon.NomenclaturalStatus.Should().Be("valid");
            beechSpeciesTaxon.TaxonomicStatus.Should().Be("accepted");
            beechSpeciesTaxon.DynamicProperties.DyntaxaTaxonId.Should().Be(220778);
            beechSpeciesTaxon.HigherClassification.Should().BeNull("because this isn't calculated in this step. It is calculated in ProcessTaxa job.");

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech (bok) species has one recommended swedish vernacular name,
            //          two swedish synonyms and one english synonym
            //-----------------------------------------------------------------------------------------------------------
            var expectedBeechSpeciesTaxonVernacularNames = CreateExpectedBeechSpeciesTaxonVernacularNames();
            beechSpeciesTaxon.VernacularNames.Should().BeEquivalentTo(expectedBeechSpeciesTaxonVernacularNames);

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Beech (bok) genus has one main parent and one secondary parent.
            //-----------------------------------------------------------------------------------------------------------
            var beechGenusTaxon = taxa.Single(t => t.Id == 1006037);
            beechGenusTaxon.DynamicProperties.ParentDyntaxaTaxonId.Should().Be(2002757); // Family - Fagaceae (bokväxter)
            beechGenusTaxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds.Should().BeEquivalentTo(new List<int> { 6005257 }); // Organism group - Hardwood forest trees (ädellövträd)
        }

        private List<DarwinCoreVernacularName> CreateExpectedBeechSpeciesTaxonVernacularNames()
        {
            var beechVernacularNames = new List<DarwinCoreVernacularName>
            {
                new DarwinCoreVernacularName { VernacularName = "bok", Language = "sv", CountryCode = "SE", IsPreferredName = true, Source = "Dyntaxa. Svensk taxonomisk databas", TaxonID = "urn:lsid:dyntaxa.se:Taxon:220778", TaxonRemarks = ""},
                new DarwinCoreVernacularName { VernacularName = "vanlig bok", Language = "sv", CountryCode = "SE", IsPreferredName = false, Source = "Dyntaxa. Svensk taxonomisk databas", TaxonID = "urn:lsid:dyntaxa.se:Taxon:220778", TaxonRemarks = "" },
                new DarwinCoreVernacularName { VernacularName = "rödbok", Language = "sv", CountryCode = "SE", IsPreferredName = false, Source = "Dyntaxa. Svensk taxonomisk databas", TaxonID = "urn:lsid:dyntaxa.se:Taxon:220778", TaxonRemarks = "" },
                new DarwinCoreVernacularName { VernacularName = "Beech", Language = "en", CountryCode = "GB", IsPreferredName = false, Source = "Dyntaxa. Svensk taxonomisk databas", TaxonID = "urn:lsid:dyntaxa.se:Taxon:220778", TaxonRemarks = "" }
            };

            return beechVernacularNames;
        }
    }
}
