using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.UnitTests.TestHelpers.Fixtures;
using Xunit;

namespace SOS.Lib.UnitTests.Models.TaxonTree
{
    public class TaxonTreeTests : IClassFixture<ProcessedBasicTaxaFixture>
    {
        private readonly ProcessedBasicTaxaFixture _fixture;

        public TaxonTreeTests(ProcessedBasicTaxaFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Ichthyaetus_genus_has_5_underlying_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int ichthyaetusTaxonId = 6011885;
            int[] expectedUnderlyingTaxonIds = { 266836, 103067, 266238, 267106, 266835 };
            var sut = TaxonTreeFactory.CreateTaxonTree(_fixture.Taxa);
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var underlyingTaxa = sut.GetUnderlyingTaxonIds(ichthyaetusTaxonId, false).ToArray();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            underlyingTaxa.Should().HaveCount(5);
            underlyingTaxa.Should().BeEquivalentTo(expectedUnderlyingTaxonIds);
        }
    }
}
