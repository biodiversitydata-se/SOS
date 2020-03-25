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
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class DwcArchiveReaderBatchIntegrationTests
    {
        private const string PsophusStridulusArchivePath = "./resources/dwca/dwca-occurrence-lifewatch-psophus-stridulus.zip";
        private const string DwcArchiveWithEmofExtension = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
        private const string SamplingEventDwcArchiveWithMofExtension = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
        private const string ArtportalenDwcArchiveExportedFromAnalysisPortal = "./resources/dwca/dwca-occurrence-emof-lifewatch-artportalen.zip";

        [Fact]
        public async Task Read_artportalen_occurrence_dwc_archive_observations_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const int batchSize = 10000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatches(ArtportalenDwcArchiveExportedFromAnalysisPortal, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(35670);
        }


        [Fact]
        public async Task Read_psophus_stridulus_occurrence_dwc_archive_observations_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const int batchSize = 1000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatches(PsophusStridulusArchivePath, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

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
        public async Task Read_occurrence_dwc_archive_with_emof_extension_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const int batchSize = 1000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatches(DwcArchiveWithEmofExtension, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

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
        public async Task Read_sampling_event_dwc_archive_with_mof_extension_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const int batchSize = 10000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatches(SamplingEventDwcArchiveWithMofExtension, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(105872);
        }

        /// <summary>
        /// This test uses the following DwC-A: http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds&v=19.3
        /// containing 6,706,047 records.
        /// Reading all observations and storing them in RAM will use more than 10GB, so reading in batches is necessary.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_local_large_occurrence_dwc_archive_observations_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const string ringedBirdsDwcArchive = @"C:\DwC-A\SOS dev\dwca-nrm-ringedbirds-v19.3.zip";
            const int batchSize = 50000;
            const int totalNrObservationsToRead = 250000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatches(ringedBirdsDwcArchive, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
                if (observations.Count >= totalNrObservationsToRead) break;
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(totalNrObservationsToRead);
        }
    }
}