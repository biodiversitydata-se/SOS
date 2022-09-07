using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MvmService;
using SOS.Harvest.Services;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Services
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