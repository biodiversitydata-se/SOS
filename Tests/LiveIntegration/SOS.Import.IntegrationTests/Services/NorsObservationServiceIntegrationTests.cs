using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Services
{
    public class NorsObservationServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var norsObservationService = new NorsObservationService(
                new AquaSupportRequestService(new HttpClientService(new NullLogger<HttpClientService>()), new NullLogger<AquaSupportRequestService>()),
                importConfiguration.NorsServiceConfiguration,
                new NullLogger<NorsObservationService>());
            var startDate = new DateTime(1988, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);
            var changeId = 0L;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var xmlDocument = await norsObservationService.GetAsync(startDate, endDate, changeId);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var xmlStr = xmlDocument.ToString();
            xmlStr.Should().NotBeNull();
        }
    }
}