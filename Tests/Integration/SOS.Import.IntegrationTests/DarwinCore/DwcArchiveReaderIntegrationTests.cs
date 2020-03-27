using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DwcArchiveReaderIntegrationTests
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
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(ArtportalenDwcArchiveExportedFromAnalysisPortal, batchSize);
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
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(PsophusStridulusArchivePath, batchSize);
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
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(DwcArchiveWithEmofExtension, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(25816);
            var obs = observations.Single(o => o.RecordId == "urn:lsid:artportalen.se:Sighting:69400054");
            obs.ExtendedMeasurementOrFacts.Single(m => m.MeasurementType == "Teknik")
                .MeasurementValue.Should().Be("D240X");
        }

        [Fact]
        public async Task Read_event_dwc_archive_with_emof_extension_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const string archivePath = "./resources/dwca/dwca-event-emof-vims_neamap.zip";
            const int batchSize = 10000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(archivePath, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
            {
                observations.AddRange(verbatimObservationsBatch);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(37201);
            var obs = observations.Single(o => o.OccurrenceID == "e3cc741e-23e6-11e8-a64f-4f26032957e3");
            obs.EventExtendedMeasurementOrFacts
                .Count().Should().Be(6, "because there are 6 different event measurements for the 'Station:NM20130901150' event");
            obs.EventExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "air temperature")
                .MeasurementValue.Should().Be("20");
            obs.EventExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "wind speed")
                .MeasurementValue.Should().Be("9");

            obs.ExtendedMeasurementOrFacts
                .Count().Should().Be(3, "because there are 3 different occurrence measurements for the 'e3cc741e-23e6-11e8-a64f-4f26032957e3' occurrence.");
            obs.ExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "catch per unit effort count")
                .MeasurementValue.Should().Be("1.304408918");
            obs.ExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "catch per unit effort biomass")
                .MeasurementValue.Should().Be("0.632638325");
            obs.ExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "total number of individuals caught")
                .MeasurementValue.Should().Be("1");

        }

        [Fact]
        public async Task Read_event_dwc_archive_with_emof_extension_in_batches_as_DwcEvent_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            const string archivePath = "./resources/dwca/dwca-event-emof-vims_neamap.zip";
            const int batchSize = 1000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var eventBatches = dwcArchiveReader.ReadSamplingEventArchiveInBatchesAsDwcEventAsync(archivePath, batchSize);
            List<DwcEvent> dwcEvents = new List<DwcEvent>();
            await foreach (List<DwcEvent> verbatimObservationsBatch in eventBatches)
            {
                dwcEvents.AddRange(verbatimObservationsBatch);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dwcEvents.Count.Should().Be(1970);
            var dwcEvent = dwcEvents.Single(e => e.EventID == "Station:NM20130901150");
            dwcEvent.ExtendedMeasurementOrFacts
                .Count().Should().Be(6, "because there are 6 different event measurements for the 'Station:NM20130901150' event");
            dwcEvent.ExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "air temperature")
                .MeasurementValue.Should().Be("20");
            dwcEvent.ExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "wind speed")
                .MeasurementValue.Should().Be("9");
        }


        /// <summary>
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
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(SamplingEventDwcArchiveWithMofExtension, batchSize);
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
            const int totalNrObservationsToRead = 150000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationBatches = dwcArchiveReader.ReadArchiveInBatchesAsync(ringedBirdsDwcArchive, batchSize);
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