using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.UnitTests.TestHelpers.Factories;
using SOS.Lib.UnitTests.TestHelpers.Fixtures;
using Xunit;

namespace SOS.Lib.UnitTests.Models.TaxonTree
{
    public class DarwinCoreTaxonTreeTests : IClassFixture<MinimalDarwinCoreTaxaFixture>
    {
        private readonly MinimalDarwinCoreTaxaFixture _fixture;

        public DarwinCoreTaxonTreeTests(MinimalDarwinCoreTaxaFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Ichthyaetus_genus_has_5_underlying_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int ichthyaetusTaxonId = 6011885;
            int[] expectedUnderlyingTaxonIds = { 266836, 103067, 266238, 267106, 266835 };
            var sut = TaxonTreeFactory.CreateTaxonTree<object>(_fixture.Taxa);
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var underlyingTaxa = sut.GetUnderlyingTaxonIds(ichthyaetusTaxonId, false);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            underlyingTaxa.Should().HaveCount(5);
            underlyingTaxa.Should().BeEquivalentTo(expectedUnderlyingTaxonIds);
        }
    }
}
