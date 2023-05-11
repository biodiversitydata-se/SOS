using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Harvesters.Shark;
using SOS.Harvest.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class SharkObservationHarvesterIntegrationTests : TestBase
    {        
        [Fact]
        [Trait("Category", "Integration")]
        public async Task Get_shark_harvest_datasets()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var sharkServiceConfiguration = new SharkServiceConfiguration
            {
                BaseAddress = "https://sharkdata.smhi.se",
                AcceptHeaderContentType = "application/json",
                ValidDataTypes = new List<string>
                {
                    "Epibenthos",
                    "GreySeal",
                    "HarbourSeal",
                    "Phytoplankton",
                    "RingedSeal",
                    "Zoobenthos",
                    "Zooplankton"
                }
            };
            var httpClientService = new HttpClientService(new NullLogger<HttpClientService>());
            SharkObservationService sharkObservationService = new SharkObservationService(httpClientService, 
                sharkServiceConfiguration, new NullLogger<SharkObservationService>());
            var sharkObservationHarvester = new SharkObservationHarvester(
                sharkObservationService,
                new Mock<ISharkObservationVerbatimRepository>().Object,
                sharkServiceConfiguration,
                new NullLogger<SharkObservationHarvester>());                

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasets = await sharkObservationHarvester.GetDatasetsToHarvestAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            datasets.Should().NotBeNull();
        }       
    }
}