using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Process.UnitTests.Services
{
    public class iNaturalistApiObservationServiceTests
    {
        public iNaturalistApiObservationServiceTests()
        {
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        public async Task GetINaturalistObservations_SpecificPage()
        {
            // Arrange
            var iNatService = new iNaturalistApiObservationService(new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), new NullLogger<iNaturalistApiObservationService>());

            // Act
            var result = await iNatService.GetAsync(null, null, 1);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        public async Task GetINaturalistObservations_IterateByIdAbove()
        {
            // Arrange
            var iNatService = new iNaturalistApiObservationService(new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), new NullLogger<iNaturalistApiObservationService>());
            int idAbove = 236000000;

            // Act
            var observations = new List<iNaturalistVerbatimObservation>();
            await foreach (var pageResult in iNatService.GetByIterationAsync(idAbove))
            {
                observations.AddRange(pageResult.Observations);
            }

            // Assert
            observations.Should().NotBeNull();
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        public async Task GetINaturalistObservations_IterateByUpdatedFromDate()
        {
            // Arrange
            var iNatService = new iNaturalistApiObservationService(new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), new NullLogger<iNaturalistApiObservationService>());
            DateTime updatedFromDate = DateTime.Now - TimeSpan.FromDays(1);

            // Act
            var observations = new List<iNaturalistVerbatimObservation>();
            await foreach (var pageResult in iNatService.GetByIterationAsync(updatedFromDate))
            {
                observations.AddRange(pageResult.Observations);
            }

            // Assert
            observations.Should().NotBeNull();
        }
    }
}