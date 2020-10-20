using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IDarwinCoreArchiveVerbatimRepository : IVerbatimRepositoryBase<DwcObservationVerbatim, ObjectId>
    {
        Task<bool> AddCollectionAsync(IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, IIdIdentifierTuple idIdentifierTuple);
        Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, string collectionName);

        Task<bool> CheckIfCollectionExistsAsync(
            int dataProviderId,
            string dataProviderIdentifier);

        Task<bool> DeleteCollectionAsync(IIdIdentifierTuple idIdentifierTuple);

        Task<IAsyncCursor<DwcObservationVerbatim>> GetAllByCursorAsync(
            int dataProviderId,
            string dataProviderIdentifier);

        List<DistinictValueCount<string>> GetDistinctValuesCount(
            string collectionName,
            Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
            int limit);

        Task<bool> ClearTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple);

        Task<bool> AddManyToTempHarvestAsync(IEnumerable<DwcObservationVerbatim> items,
            IIdIdentifierTuple idIdentifierTuple);

        Task<bool> RenameTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple);
        Task DeleteTempHarvestCollectionIfExists(IIdIdentifierTuple idIdentifierTuple);
    }
}