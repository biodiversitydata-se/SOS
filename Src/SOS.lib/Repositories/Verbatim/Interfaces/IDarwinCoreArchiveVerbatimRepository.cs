using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IDarwinCoreArchiveVerbatimRepository : IVerbatimRepositoryBase<DwcObservationVerbatim, int>
    {
        /// <summary>
        /// Count distinct values
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="expression"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
            int limit);
    }
}