using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Security;
using SOS.Import.DarwinCore;
using SOS.Import.Harvesters.Observations;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class DwcArchiveReaderIntegrationTests
    {
        private const string PsophusStridulusArchivePath = "./resources/dwca/dwca-occurrence-lifewatch-psophus-stridulus.zip";
        private const string DwcArchiveWithEmofExtension = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
        private const string SamplingEventDwcArchiveWithMofExtension = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";

        [Fact]
        public async Task Read_psophus_stridulus_occurrence_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await dwcArchiveReader.ReadArchiveAsync(PsophusStridulusArchivePath);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(2158);
        }

        /// <summary>
        /// todo - handle emof extension.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_occurrence_dwc_archive_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await dwcArchiveReader.ReadArchiveAsync(DwcArchiveWithEmofExtension);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(25816);
        }

        /// <summary>
        /// todo - handle measurementOrFact extension.
        /// Some links to Sampling Event Data Dwc-A:
        /// - https://github.com/gbif/ipt/wiki/BestPracticesSamplingEventData
        /// - https://www.gbif.org/data-quality-requirements-sampling-events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_sampling_event_dwc_archive_with_mof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await dwcArchiveReader.ReadArchiveAsync(SamplingEventDwcArchiveWithMofExtension);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(105872);
        }

        /// <summary>
        /// This test uses the following DwC-A: http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds&v=19.3
        /// containing 6,706,047 records.
        ///
        /// With the current implementation (reading all observations into RAM) the amount of RAM usage is too high > 11GB
        /// Need to add a new function that reads the observations in batches.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_local_large_occurrence_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const string ringedBirdsDwcArchive = @"C:\DwC-A\SOS dev\dwca-nrm-ringedbirds-v19.3.zip";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observations = await dwcArchiveReader.ReadArchiveAsync(ringedBirdsDwcArchive);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Should().NotBeNull();
        }

    }
}
