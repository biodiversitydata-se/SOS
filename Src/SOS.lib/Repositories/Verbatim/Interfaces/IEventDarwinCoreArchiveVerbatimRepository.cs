using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    public interface IEventDarwinCoreArchiveVerbatimRepository : IVerbatimRepositoryBase<DwcEventVerbatim, int>
    {
        /// <summary>
        /// Count distinct values
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcEventVerbatim, DistinctValueObject<string>>> expression,
            int limit);
    }

    public interface IEventOccurrenceDarwinCoreArchiveVerbatimRepository : IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>
    {
        /// <summary>
        /// Count distinct values
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcEventOccurrenceVerbatim, DistinctValueObject<string>>> expression,
            int limit);
    }

}