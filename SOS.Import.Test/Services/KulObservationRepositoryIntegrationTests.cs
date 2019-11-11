using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using KulService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Import.Test.Services
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
            var kulObservationService = new KulObservationService(
                new Mock<ILogger<KulObservationService>>().Object, 
                importConfiguration.KulServiceConfiguration);
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