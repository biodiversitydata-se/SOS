﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.DarwinCore
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
            var dwcArchiveReader = new DwcOccurrenceArchiveReader();
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 100,
                Identifier = "TestLifeWatchArtportalen"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcArchiveReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(35670);
            observations.First().DataProviderId.Should().Be(dataProviderIdIdentifierTuple.Id);
            observations.First().DataProviderIdentifier.Should().Be(dataProviderIdIdentifierTuple.Identifier);
        }

        /// <summary>
        ///     This test uses the following DwC-A: http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds&v=19.3
        ///     containing 6,706,047 records.
        ///     Reading all observations and storing them in RAM will use more than 10GB, so reading in batches is necessary.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_local_large_occurrence_dwca_in_batches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader();
            const string archivePath = @"C:\DwC-A\SOS dev\dwca-nrm-ringedbirds-v19.3.zip";
            const int batchSize = 50000;
            const int totalNrObservationsToRead = 150000;
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 105,
                Identifier = "TestRingedBirds"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observationsBatches =
                dwcOccurrenceArchiveReader.ReadArchiveInBatchesAsync(archiveReader, dataProviderIdIdentifierTuple,
                    batchSize);
            var observations = new List<DwcObservationVerbatim>();
            await foreach (var observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
                if (observations.Count >= totalNrObservationsToRead) break;
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(totalNrObservationsToRead);
        }

        [Fact]
        public async Task Read_occurrence_dwca_with_audubon_media_description_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-audubonmedia-rrel-cumv_amph.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader();
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 102,
                Identifier = "TestAudubonMedia"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations =
                await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(13380);
            var obs = observations.Single(o => o.CatalogNumber == "6021");
            obs.ObservationAudubonMedia.Count.Should().Be(1);
            var mediaItem = obs.ObservationAudubonMedia.First();
            mediaItem.Identifier.Should().Be("http://arctos.database.museum/media/10451700");
            mediaItem.AccessURI.Should().Be("http://arctos.database.museum/media/10451700?open");
            mediaItem.Format.Should().Be("image/jpeg");
            mediaItem.FormatAc.Should().Be("image/jpeg");
            mediaItem.Type.Should().Be("image");
            mediaItem.TypeAc.Should().Be("image");
            mediaItem.WebStatement.Should().Be("http://creativecommons.org/licenses/by-nc-sa/3.0");
            mediaItem.Modified.Should().Be("2017-12-12 11:50:34.0");
        }

        [Fact]
        public async Task Read_occurrence_dwca_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader();
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 103,
                Identifier = "TestLifeWatchEmof"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations =
                await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);

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
            const string archivePath = "./resources/dwca/measurementorfact/bdrs1.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader();
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 104,
                Identifier = "TestMeasurementOrFact"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations =
                await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(2);
            observations[0].ObservationMeasurementOrFacts.Should().NotBeNull();
        }

        [Fact]
        public async Task Read_occurrence_dwca_with_multimedia_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-occurrence-multimedia-verbatimoccurrence.zip";
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader();
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 101,
                Identifier = "TestMultimedia"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations =
                await dwcOccurrenceArchiveReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);

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
    }
}