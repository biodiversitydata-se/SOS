using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class CultureCodeHelperTests
    {
        [Theory]
        [InlineData("sv-SE", "sv-SE")]
        [InlineData("sv-se", "sv-SE")]
        [InlineData("sv", "sv-SE")]
        [InlineData("SV", "sv-SE")]
        [InlineData("swe", "sv-SE")]
        [InlineData("", "sv-SE")]
        [InlineData(null, "sv-SE")]
        [InlineData("en-GB", "en-GB")]
        [InlineData("en-gb", "en-GB")]
        [InlineData("en", "en-GB")]
        [InlineData("EN", "en-GB")]
        [InlineData("eng", "en-GB")]
        public void Resolve_cultureCode(
            string cultureCode,
            string expected)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = CultureCodeHelper.GetCultureCode(cultureCode);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
        }
    }
}