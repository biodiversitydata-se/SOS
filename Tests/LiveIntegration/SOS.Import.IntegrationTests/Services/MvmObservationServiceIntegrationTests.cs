using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MvmService;
using SOS.Harvest.Services;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Services
{
    public class MvmObservationServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var changeClient = new SpeciesObservationChangeServiceClient();
            var mvmObservationService = new MvmObservationService(changeClient,
                importConfiguration.MvmServiceConfiguration, new NullLogger<MvmObservationService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await mvmObservationService.GetAsync(0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Observations.Should().NotBeEmpty();
        }
    }
}