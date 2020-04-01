using System;
using System.Collections.Generic;
using System.Linq;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    /// <summary>
    /// DwC-A reader.
    /// </summary>
    public class DwcArchiveReader : Interfaces.IDwcArchiveReader
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            int batchSize)
        {
            var filename = System.IO.Path.GetFileName(archiveReader.FileName);
            var occurrenceReader = CreateOccurrenceReader(archiveReader.CoreFile.FileMetaData.RowType);
            await foreach (var batch in occurrenceReader.ReadArchiveInBatchesAsync(archiveReader, batchSize, filename))
            {
                yield return batch;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<List<DwcEvent>> ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
            ArchiveReader archiveReader,
            int batchSize)
        {
            var filename = System.IO.Path.GetFileName(archiveReader.FileName);
            var dwcSamplingEventArchiveReader = new DwcSamplingEventArchiveReader(_logger);
            await foreach (var batch in dwcSamplingEventArchiveReader.ReadArchiveInBatchesAsync(archiveReader, batchSize, filename))
            {
                yield return batch;
            }
        }

        private IDwcArchiveReaderAsDwcObservation CreateOccurrenceReader(string rowType)
        {
            if (rowType == RowTypes.Occurrence)
            {
                return new DwcOccurrenceArchiveReader(_logger);
            }
            else // Event
            {
                return new DwcOccurrenceSamplingEventArchiveReader(_logger);
            }
        }

        /// <summary>
        /// Validate a DwC-A file.
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