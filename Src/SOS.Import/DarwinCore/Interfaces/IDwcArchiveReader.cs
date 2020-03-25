using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Interfaces
{
    public interface IDwcArchiveReader
    {
        Task<List<DwcObservationVerbatim>> ReadArchiveAsync(string archivePath);
        IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatches(string archivePath, int batchSize);
    }
}
