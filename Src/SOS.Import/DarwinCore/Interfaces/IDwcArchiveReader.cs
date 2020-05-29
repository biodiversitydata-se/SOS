using System.Collections.Generic;
using System.Threading.Tasks;
using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Interfaces
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
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize);

        /// <summary>
        ///     Reads a DwC-A file and returns the observations.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        public Task<List<DwcObservationVerbatim>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple);

        /// <summary>
        ///     Reads a Sampling Event DwC-A and returns the events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcEvent>> ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize);

        /// <summary>
        ///     Reads a Sampling Event DwC-A and returns the events.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        public Task<List<DwcEvent>> ReadSamplingEventArchiveAsDwcEventAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple);
    }
}