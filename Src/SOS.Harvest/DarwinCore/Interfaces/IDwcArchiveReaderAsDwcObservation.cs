using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Interfaces
{
    public interface IDwcArchiveReaderAsDwcObservation
    {
        IAsyncEnumerable<List<DwcObservationVerbatim>?> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize);

        /// <summary>
        /// Read event verbatim
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEvents(ArchiveReader archiveReader,
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
        /// Read occurrences in batches.
        /// </summary>
        /// <param name="archiveReaderContext"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcObservationVerbatim>?> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext);
        
        /// <summary>
        /// Read events.
        /// </summary>
        /// <param name="archiveReaderContext"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEvents(ArchiveReaderContext archiveReaderContext);
    }
}