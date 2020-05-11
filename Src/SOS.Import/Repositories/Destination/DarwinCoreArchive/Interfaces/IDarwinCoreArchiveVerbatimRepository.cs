using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Import.DarwinCore;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDarwinCoreArchiveVerbatimRepository : IVerbatimRepository<DwcObservationVerbatim, ObjectId>
    {
        Task<bool> DeleteCollectionAsync(DwcaDatasetInfo datasetInfo);
        Task<bool> AddCollectionAsync(DwcaDatasetInfo datasetInfo);
        Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, DwcaDatasetInfo datasetInfo);
        Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, string collectionName);
        List<DistinictValueCount<string>> GetDistinctValuesCount(
            string collectionName,
            Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
            int limit);
    }
}
