using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Interfaces
{
    public interface IDwcArchiveReader
    {
        /// <summary>
        /// Reads a DwC-A file and returns the observations in batches.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(string archivePath, int batchSize);

        /// <summary>
        /// Reads a Sampling Event DwC-A and returns the events in batches.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcEvent>> ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
            string archivePath,
            int batchSize);
    }
}