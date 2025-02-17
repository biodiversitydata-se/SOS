using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Harvesters.AquaSupport.Sers;
using SOS.Harvest.Services;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Services;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Services
{
    public class SersObservationServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestHarvestSers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var sersObservationService = new SersObservationService(
                new AquaSupportRequestService(new HttpClientService(new NullLogger<HttpClientService>()), new NullLogger<AquaSupportRequestService>()),
                importConfiguration.SersServiceConfiguration,
                new NullLogger<SersObservationService>());
            var sersObservationHarvester = new SersObservationHarvester(sersObservationService,
                new Mock<ISersObservationVerbatimRepository>().Object,
                importConfiguration.SersServiceConfiguration,
                new NullLogger<SersObservationHarvester>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var result = await sersObservationHarvester.HarvestObservationsAsync(new DataProvider(), null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }
    }
}