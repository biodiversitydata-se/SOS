using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IDarwinCoreArchiveEventRepository : IVerbatimRepositoryBase<DwcEvent, ObjectId>
    {
        Task<bool> DeleteCollectionAsync(IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddCollectionAsync(IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, string collectionName);
        Task<bool> ClearTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddManyToTempHarvestAsync(IEnumerable<DwcEvent> items, IIdIdentifierTuple idIdentifierTuple);
        Task<bool> RenameTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple);
    }
}