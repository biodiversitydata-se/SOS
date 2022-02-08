using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class GeoJsonHelperTests
    {
        [Theory]
        [InlineData(10000, 620000, 6870000, "10kmN62E687")]
        [InlineData(15000, 625000, 6870000, "15kmN625E6870")]
        [InlineData(1720, 621700, 6870000, "1.72kmN62170E687000")]
        [InlineData(500, 620500, 6870000, "500mN6205E68700")]
        public void Test_GetGridCellId(
            int gridCellSizeInMeters,
            int lowerLeftX,
            int lowerLeftY,             
            string expected)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sp = System.Diagnostics.Stopwatch.StartNew();
            string result = GeoJsonHelper.GetGridCellId(gridCellSizeInMeters, lowerLeftX, lowerLeftY);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
        }
    }
}