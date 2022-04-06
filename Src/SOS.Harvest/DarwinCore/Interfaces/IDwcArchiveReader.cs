﻿using DwC_A;
using SOS.Lib.Models.Interfaces;
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
        IAsyncEnumerable<IEnumerable<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
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
        Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadSamplingEventArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple);
    }
}