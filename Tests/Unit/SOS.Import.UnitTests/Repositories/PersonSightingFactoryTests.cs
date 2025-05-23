﻿using FluentAssertions;
using SOS.Harvest.Factories;
using Xunit;

namespace SOS.Import.UnitTests.Repositories
{
    public class PersonSightingFactoryTests
    {
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(null, null, null, null, null, null, null, null)]
        [InlineData("Art Vandelay", null, null, null, null, null, null, "Art Vandelay")]
        [InlineData("   Art Vandelay   ", null, null, null, null, null, null, "Art Vandelay")]
        [InlineData("Art Vandelay", null, null, null, null, null, 2019, "Art Vandelay")]
        [InlineData("Art Vandelay", null, null, null, null, "confirmatorText", null,
            "Art Vandelay # Conf. confirmatorText")]
        [InlineData("Art Vandelay", null, null, null, null, "confirmatorText", 2018,
            "Art Vandelay # Conf. confirmatorText 2018")]
        [InlineData(null, null, null, null, null, "confirmatorText", null, "Conf. confirmatorText")]
        [InlineData(null, null, null, "determinationDescription", null, null, null, "determinationDescription")]
        [InlineData("Art Vandelay", "determinerText", 2019, null, null, null, null, "Art Vandelay determinerText 2019")]
        [InlineData("Art Vandelay", "determinerText", 2019, "determinerDescription", "Kel Varnsen", null, null,
            "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen")]
        [InlineData("Art Vandelay", "determinerText", 2019, "determinerDescription", "Kel Varnsen", "confirmatorText",
            2018, "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen confirmatorText 2018")]
        [InlineData("  Art Vandelay  ", "  determinerText  ", 2019, "  determinerDescription  ", "  Kel Varnsen  ",
            "  confirmatorText  ", 2018,
            "Art Vandelay determinerText 2019 # determinerDescription # Conf. Kel Varnsen confirmatorText 2018")]
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
            var verifiedBy = PersonSightingFactory.ConcatenateVerifiedByString(
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
    }
}