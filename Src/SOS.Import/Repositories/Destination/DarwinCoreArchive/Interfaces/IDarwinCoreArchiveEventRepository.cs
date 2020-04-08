using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Import.DarwinCore;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDarwinCoreArchiveEventRepository : IVerbatimRepository<DwcEvent, ObjectId>
    {
        Task<bool> DeleteCollectionAsync(DwcaDatasetInfo datasetInfo);
        Task<bool> AddCollectionAsync(DwcaDatasetInfo datasetInfo);
        Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, DwcaDatasetInfo datasetInfo);
    }
}
