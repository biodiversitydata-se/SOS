using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Interfaces
{
    public interface IDwcArchiveReader
    {
        /// <summary>
        /// Reads a DwC-A file and returns the observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="datasetInfo"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            DwcaDatasetInfo datasetInfo,
            int batchSize);

        /// <summary>
        /// Reads a Sampling Event DwC-A and returns the events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="datasetInfo"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        IAsyncEnumerable<List<DwcEvent>> ReadSamplingEventArchiveInBatchesAsDwcEventAsync(
            ArchiveReader archiveReader,
            DwcaDatasetInfo datasetInfo,
            int batchSize);
    }
}