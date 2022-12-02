using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Interfaces
{
    public interface IDwcArchiveReaderAsDwcObservation
    {
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize);

        /// <summary>
        /// Read event verbatim
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadEvents(ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple);

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        Task<List<DwcVerbatimObservationDataset>> ReadDatasetsAsync(ArchiveReader archiveReader);

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        Task<List<DwcVerbatimObservationDataset>> ReadDatasetsAsync(ArchiveReaderContext archiveReaderContext);

        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext);
        Task<IEnumerable<DwcEventVerbatim>> ReadEvents(ArchiveReaderContext archiveReaderContext);
    }
}