using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SOS.Import.Extensions;
using SOS.Import.Factories;
using SOS.Import.UnitTests.TestData;
using Xunit;

namespace SOS.Import.UnitTests.Repositories
{
    public class PersonSightingFactoryTests
    {
        [Theory]
        [Trait("Category","Unit")]
        [InlineData(null, null, null, null, null, null, null, null)]
        [InlineData("Art Vandelay", null, null, null, null, null, null, "Art Vandelay")]
        [InlineData("   Art Vandelay   ", null, null, null, null, null, null, "Art Vandelay")]
        [InlineData("Art Vandelay", null, null, null, null, null, 2019, "Art Vandelay")]
        [InlineData("Art Vandelay", null, null, null, null, "confirmatorText", null, "Art Vandelay # Conf. confirmatorText")]
        [InlineData("Art Vandelay", null, null, null, null, "confirmatorText", 2018, "Art Vandelay # Conf. confirmatorText 2018")]
        [InlineData(null, null, null, null, null, "confirmatorText", null, "Conf. confirmatorText")]
        [InlineData(null, null, null, "determinationDescription", null, null, null, "determinationDescription")]
        [InlineData("Art Vandelay", "determinerText", 2019, null, null, null, null, "Art Vandelay determinerText 2019")]
        [InlineData("Art Vandelay", "determinerText", 2019, "determinerDescription", "Kel Varnsen", null, null, "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen")]
        [InlineData("Art Vandelay", "determinerText", 2019, "determinerDescription", "Kel Varnsen", "confirmatorText", 2018, "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen confirmatorText 2018")]
        [InlineData("  Art Vandelay  ", "  determinerText  ", 2019, "  determinerDescription  ", "  Kel Varnsen  ", "  confirmatorText  ", 2018, "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen confirmatorText 2018")]

        public void TestConcatenateVerifiedByString(
            string determinerName,
            string determinerText,
            int? determinerYear,
            string determinationDescription,
            string confirmatorName,
            string confirmatorText,
            int? confirmatorYear,
            string expected)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            string verifiedBy = PersonSightingFactory.ConcatenateVerifiedByString(
                determinerName,
                determinerText,
                determinerYear,
                determinationDescription,
                confirmatorName,
                confirmatorText,
                confirmatorYear);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verifiedBy.Should().Be(expected);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TestGetVerifiedByDataDictionary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var sightingRelationEntities = SightingRelationEntityTestData.CreateItems().ToVerbatims().ToList();
            var speciesCollectionItemEntities = SpeciesCollectionItemEntityTestData.CreateItems();
            var personByUserId = PersonTestData.CreatePersonDictionary();
            var sightingIds = new HashSet<int>(sightingRelationEntities
                .Select(x => x.SightingId)
                .Concat(speciesCollectionItemEntities.Select(x => x.SightingId))
                .Distinct());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = PersonSightingFactory.CalculatePersonSightingDictionary(
                sightingIds,
                personByUserId,
                null,
                speciesCollectionItemEntities.ToVerbatims().ToList(),
                sightingRelationEntities);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().ContainKeys(sightingIds);
            result[1].Observers.Should().Be("Via Tord Yvel", "because Observer is not specified, but ReportedBy is");
            result[1].ReportedBy.Should().Be("Tord Yvel", "because ReportedBy is specified");
            result[1].SpeciesCollection.Should().BeNull("because SpeciesCollection is not specified");
        }
    }
}
