using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Interfaces
{
    public interface IDwcArchiveReader
    {
        /// <summary>
        ///     Reads a DwC-A file and returns the observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>?> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize = 100000);

        /// <summary>
        ///     Reads a DwC-A file and returns the observations.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="maxNrObservationsToReturn">Max number of observations to return.</param>
        /// <returns></returns>
        Task<IEnumerable<DwcObservationVerbatim>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int maxNrObservationsToReturn = int.MaxValue);

        /// <summary>
        ///     Reads a Sampling Event DwC-A and returns the events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadSamplingEventArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple);

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReader archiveReader);

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReaderContext archiveReaderContext);
        
        /// <summary>
        /// Read occurrences.
        /// </summary>
        /// <param name="archiveReaderContext"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcObservationVerbatim>?> ReadOccurrencesAsync(ArchiveReaderContext archiveReaderContext);
        
        /// <summary>
        /// Read occurrences in batches.
        /// </summary>
        /// <param name="archiveReaderContext"></param>
        /// <returns></returns>
        IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>?> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext);
        
        /// <summary>
        /// Read events.
        /// </summary>
        /// <param name="archiveReaderContext"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEventsAsync(ArchiveReaderContext archiveReaderContext);

        /// <summary>
        /// Set initilaize value for id
        /// </summary>
        /// <param name="value"></param>
        void SetIdInitValue(int value);
    }
}