using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim.Interfaces;

/// <summary>
/// </summary>
public interface IDarwinCoreArchiveVerbatimRepository : IVerbatimRepositoryBase<DwcObservationVerbatim, int>
{
    /// <summary>
    /// Count distinct values
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
        Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
        int limit);

    /// <summary>
    /// Get source file from MongoDB
    /// </summary>
    /// <returns></returns>
    Task<Stream> GetSourceFileAsync();

    /// <summary>
    /// Stor source file in MongoDB
    /// </summary>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    Task<bool> StoreSourceFileAsync(Stream fileStream);
}