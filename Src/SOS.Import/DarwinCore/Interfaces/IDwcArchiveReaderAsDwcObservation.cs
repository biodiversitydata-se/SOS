using System;
using System.Collections.Generic;
using System.Text;
using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Interfaces
{
    public interface IDwcArchiveReaderAsDwcObservation
    {
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            DwcaDatasetInfo datasetInfo,
            int batchSize);
    }
}
