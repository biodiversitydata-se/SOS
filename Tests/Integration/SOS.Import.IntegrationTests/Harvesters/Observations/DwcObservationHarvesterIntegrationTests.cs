using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using SOS.Import.Harvesters.Observations;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class DwcObservationHarvesterIntegrationTests
    {
        private const string PsophusStridulusArchivePath = "./resources/psophus-stridulus-lifewatch-occurrences-dwca.zip";

        [Fact]
        public async Task Harvest_psophus_stridulus_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            DwcObservationHarvester dwcObservationHarvester = new DwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await dwcObservationHarvester.HarvestObservationsAsync(PsophusStridulusArchivePath, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }

    }
}
