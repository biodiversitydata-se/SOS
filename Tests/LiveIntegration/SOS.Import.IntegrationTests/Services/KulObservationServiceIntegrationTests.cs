﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services;
using SOS.Lib.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Services
{
    public class KulObservationServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var kulObservationService = new KulObservationService(
                new AquaSupportRequestService(new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), new NullLogger<AquaSupportRequestService>()),
                importConfiguration.KulServiceConfiguration,
                new NullLogger<KulObservationService>());
            var changedFrom = new DateTime(2015, 1, 1);
            var changedTo = changedFrom.AddYears(1);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulObservationService.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }
    }
}