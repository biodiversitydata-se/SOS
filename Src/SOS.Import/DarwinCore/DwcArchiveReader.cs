using System;
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

        private IDwcArchiveReaderAsDwcObservation CreateDwcReader(string rowType)
        {
            if (rowType == RowTypes.Occurrence)
            {
                return new DwcOccurrenceArchiveReader(_logger);
            }

            return new DwcOccurrenceSamplingEventArchiveReader(_logger);
        }

        public DwcArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize = 100000)
        {
            var reader = CreateDwcReader(archiveReader.CoreFile.FileMetaData.RowType);
            await foreach (var batch in reader.ReadArchiveInBatchesAsync(archiveReader, idIdentifierTuple, batchSize))
            {
                yield return batch;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DwcObservationVerbatim>> ReadArchiveAsync(
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
        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadSamplingEventArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            var reader = CreateDwcReader(RowTypes.Event);
            return await reader.ReadEvents(archiveReader, idIdentifierTuple);
        }        
    }
}