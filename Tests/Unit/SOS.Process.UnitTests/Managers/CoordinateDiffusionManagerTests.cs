using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Process.Managers;
using Xunit;

namespace SOS.Process.UnitTests.Managers
{
    public class CoordinateDiffusionManagerTests
    {
        [Fact]
        public void Calculate_coordinate_diffusion_using_WebMercator_results_in_incorrect_diffusion()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var coordinateDiffusionManager = new CoordinateDiffusionManager();
            const int protectionLevel5 = 5;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var statsByProtectionLevel = coordinateDiffusionManager.CalculateCoordinateDiffusionStats(CoordinateSys.WebMercator);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            statsByProtectionLevel[protectionLevel5].AvgDistance.Should().BeGreaterThan(18, "because the diffusion level is 50km");
        }

        [Fact]
        public void Calculate_coordinate_diffusion_using_Sweref99Tm_results_in_correct_diffusion()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var coordinateDiffusionManager = new CoordinateDiffusionManager();
            const int protectionLevel5 = 5;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var statsByProtectionLevel = coordinateDiffusionManager.CalculateCoordinateDiffusionStats(CoordinateSys.SWEREF99_TM);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            statsByProtectionLevel[protectionLevel5].AvgDistance.Should().BeGreaterThan(18, "because the diffusion level is 50km");
        }
    }
}