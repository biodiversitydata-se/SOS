using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KulService;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Services
{
    public class KulObservationServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category","Integration")]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var speciesObservationChangeServiceClient = new SpeciesObservationChangeServiceClient();
            var kulObservationService = new KulObservationService(
                speciesObservationChangeServiceClient, 
                importConfiguration.KulServiceConfiguration,
                new Mock<ILogger<KulObservationService>>().Object);
            var changedFrom = new DateTime(2015,1,1);
            var changedTo = changedFrom.AddYears(1);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = (await kulObservationService.GetAsync(changedFrom, changedTo)).ToArray();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
            result.Count().Should().BeGreaterThan(10000, "because 2019-11-04 there were 12200 sightings from 2015-01-01 to 2016-01-01");
        }
    }
}