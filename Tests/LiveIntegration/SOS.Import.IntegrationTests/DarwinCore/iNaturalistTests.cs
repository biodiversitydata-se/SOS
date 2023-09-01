using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class iNaturalistTests
    {
        /// <summary>
        /// Reads observations from iNaturalist DwC-A export and checks how many is in Sweden.
        /// http://www.inaturalist.org/observations/gbif-observations-dwca.zip
        /// </summary>
        [Fact]
        public void Read_swedish_observations_from_iNaturalist_Dwca_fast()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string filePath = @"C:\TEMP\gbif-observations-dwca.zip";
            string pattern = @",SE,";
            var countryRegex = new Regex(pattern);
            int nrObservations = 0;
            int nrSwedishObservations = 0;
            var stopwatch = Stopwatch.StartNew();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.Equals("observations.csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using var stream = entry.Open();
                            using var reader = new StreamReader(stream, Encoding.UTF8);
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                bool isMatch = countryRegex.IsMatch(line);
                                if (isMatch)
                                {
                                    nrSwedishObservations++;
                                }

                                nrObservations++;
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            nrSwedishObservations.Should().BeGreaterThan(0);
            nrSwedishObservations.Should().BeGreaterThan(nrSwedishObservations);
        }

        /// <summary>
        /// Reads observations from iNaturalist DwC-A export. This is unfortunately really slow.
        /// http://www.inaturalist.org/observations/gbif-observations-dwca.zip
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_iNaturalist_dwca_in_batches_very_slow()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcOccurrenceArchiveReader = new DwcOccurrenceArchiveReader(new NullLogger<DwcArchiveReader>());
            const string archivePath = @"C:\TEMP\gbif-observations-dwca.zip";
            const int batchSize = 50000;
            const int totalNrObservationsToRead = 150000;
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 105,
                Identifier = "iNaturalist"
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
    }
}