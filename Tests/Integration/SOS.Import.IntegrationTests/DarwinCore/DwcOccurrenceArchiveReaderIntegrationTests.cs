﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class DwcOccurrenceArchiveReaderIntegrationTests
    {
        [Fact]
        public async Task Read_artportalen_occurrence_dwca()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-emof-lifewatch-artportalen.zip";
            var dwcArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(35670);
        }

        [Fact]
        public async Task Read_occurrence_dwca_with_multimedia_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-multimedia-verbatimoccurrence.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(2582);
            var obs = observations.Single(o => o.OccurrenceID == "0C94D544-8B14-4DA8-A3AE-70AA4FC803AF");
            obs.ObservationMultimedia.Count.Should().Be(3);
            obs.ObservationMultimedia.Single(m =>
                    m.Identifier == "http://ww2.bgbm.org/specimentool/plants/dicots/Teucrium_chamaedrys_Duerbye_1.jpg")
                .Format.Should().Be("image/scan");
        }


        [Fact]
        public async Task Read_occurrence_dwca_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(25816);
            var obs = observations.Single(o => o.RecordId == "urn:lsid:artportalen.se:Sighting:69400054");
            obs.ObservationExtendedMeasurementOrFacts.Single(m => m.MeasurementType == "Teknik")
                .MeasurementValue.Should().Be("D240X");
        }

        [Fact]
        public async Task Read_occurrence_dwca_with_mof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader("./resources/dwca/measurementorfact/bdrs1.zip");
            var observations = await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(2);
            observations[0].ObservationMeasurementOrFacts.Should().NotBeNull();
        }

        /// <summary>
        /// This test uses the following DwC-A: http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds&v=19.3
        /// containing 6,706,047 records.
        /// Reading all observations and storing them in RAM will use more than 10GB, so reading in batches is necessary.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_local_large_occurrence_dwca_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());
            const string archivePath = @"C:\DwC-A\SOS dev\dwca-nrm-ringedbirds-v19.3.zip";
            const int batchSize = 50000;
            const int totalNrObservationsToRead = 150000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observationsBatches = dwcOccurrenceArchiveReader.ReadArchiveInBatchesAsync(archiveReader, batchSize);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
                if (observations.Count >= totalNrObservationsToRead) break;
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(totalNrObservationsToRead);
        }
    }
}