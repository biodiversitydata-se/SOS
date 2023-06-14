using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore
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
        public async IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>?> ReadArchiveInBatchesAsync(
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
                if (!observationsBatch?.Any() ?? true)
                {
                    continue;
                }

                observations.AddRange(observationsBatch!);
                if (observations.Count >= maxNrObservationsToReturn)
                {
                    return observations.Take(maxNrObservationsToReturn).ToList();
                }
            }

            return observations;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadSamplingEventArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            var reader = CreateDwcReader(RowTypes.Event);
            return await reader.ReadEvents(archiveReader, idIdentifierTuple);
        }


        public async Task<IEnumerable<DwcEventVerbatim>?> ReadEventsAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            var reader = CreateDwcReader(RowTypes.Event);
            return await reader.ReadEvents(archiveReader, idIdentifierTuple);
        }

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>

        public async Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReader archiveReader)
        {
            var reader = CreateDwcReader(archiveReader.CoreFile.FileMetaData.RowType);
            var datasets = await reader.ReadDatasetsAsync(archiveReader);
            return datasets;
        }

        public async Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReaderContext archiveReaderContext)
        {
            if (archiveReaderContext?.ArchiveReader?.CoreFile?.FileMetaData?.RowType == null)
            {
                return null;
            }
            var reader = CreateDwcReader(archiveReaderContext.ArchiveReader.CoreFile.FileMetaData.RowType);
            var datasets = await reader.ReadDatasetsAsync(archiveReaderContext);
            return datasets;
        }

        public async Task<IEnumerable<DwcObservationVerbatim>?> ReadOccurrencesAsync(ArchiveReaderContext archiveReaderContext)
        {            
            var observationsBatches = ReadOccurrencesInBatchesAsync(archiveReaderContext);

            var observations = new List<DwcObservationVerbatim>();
            await foreach (var observationsBatch in observationsBatches)
            {
                if (observationsBatch == null)
                {
                    continue;
                }

                observations.AddRange(observationsBatch);
                if (observations.Count >= archiveReaderContext.MaxNrObservationsToReturn)
                {
                    return observations.Take(archiveReaderContext.MaxNrObservationsToReturn).ToList();
                }
            }

            return observations;
        }

        public async IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>?> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext)
        {
            if (archiveReaderContext?.ArchiveReader?.CoreFile?.FileMetaData?.RowType == null)
            {
                yield return null;
            }

            var reader = CreateDwcReader(archiveReaderContext!.ArchiveReader!.CoreFile!.FileMetaData!.RowType);
            await foreach (var batch in reader.ReadOccurrencesInBatchesAsync(archiveReaderContext))
            {
                yield return batch;
            }
        }

        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEventsAsync(ArchiveReaderContext archiveReaderContext)
        {
            var reader = CreateDwcReader(RowTypes.Event);
            return await reader.ReadEvents(archiveReaderContext);
        }
    }
}