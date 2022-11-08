using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class GeoJsonHelperTests
    {
        [Theory]
        [InlineData(10000, 620000, 6870000, "10kmN687E62")]
        [InlineData(15000, 625000, 6870000, "15kmN6870E625")]
        [InlineData(1720, 621700, 6870000, "1.72kmN687000E62170")]
        [InlineData(500, 620500, 6870000, "500mN68700E6205")]
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