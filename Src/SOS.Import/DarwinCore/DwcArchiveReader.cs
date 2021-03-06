﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    /// <summary>
    ///     DwC-A reader.
    /// </summary>
    public class DwcArchiveReader : IDwcArchiveReader
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize = 100000)
        {
            var occurrenceReader = CreateOccurrenceReader(archiveReader.CoreFile.FileMetaData.RowType);
            await foreach (var batch in occurrenceReader.ReadArchiveInBatchesAsync(archiveReader, idIdentifierTuple,
                batchSize))
            {
                yield return batch;
            }
        }

        /// <inheritdoc />
        public async Task<List<DwcObservationVerbatim>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int maxNrObservationsToReturn = int.MaxValue)
        {
            const int batchSize = 100000;
            var observationsBatches = ReadArchiveInBatchesAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);
            var observations = new List<DwcObservationVerbatim>();
            await foreach (var observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
                if (observations.Count >= maxNrObservationsToReturn)
                {
                    return observations.Take(maxNrObservationsToReturn).ToList();
                }
            }

            return observations;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<List<DwcEvent>> ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize)
        {
            var dwcSamplingEventArchiveReader = new DwcSamplingEventArchiveReader(_logger);
            await foreach (var batch in dwcSamplingEventArchiveReader.ReadArchiveInBatchesAsync(archiveReader,
                idIdentifierTuple, batchSize))
            {
                yield return batch;
            }
        }

        /// <inheritdoc />
        public async Task<List<DwcEvent>> ReadSamplingEventArchiveAsDwcEventAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            const int batchSize = 100000;
            var observationsBatches = ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);
            var events = new List<DwcEvent>();
            await foreach (var observationsBatch in observationsBatches)
            {
                events.AddRange(observationsBatch);
            }

            return events;
        }

        private IDwcArchiveReaderAsDwcObservation CreateOccurrenceReader(string rowType)
        {
            if (rowType == RowTypes.Occurrence)
            {
                return new DwcOccurrenceArchiveReader(_logger);
            }

            return new DwcOccurrenceSamplingEventArchiveReader(_logger);
        }

        /// <summary>
        ///     Validate a DwC-A file.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="nrRows"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool TryValidateDwcACoreFile(string archivePath, out long nrRows, out string message)
        {
            nrRows = 0;
            message = null;

            try
            {
                using var archive = new ArchiveReader(archivePath);
                nrRows = archive.CoreFile.DataRows.LongCount();

                if (archive.CoreFile.FileMetaData.Id.IndexSpecified == false)
                {
                    message = "Core file is missing index of id.";
                    return false;
                }

                if (nrRows == 0)
                {
                    message = "No data rows in core file";
                    return false;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }

            return true;
        }
    }
}