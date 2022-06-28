using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     User statistics manager interface
    /// </summary>
    public interface IUserStatisticsManager
    {
        Task<IEnumerable<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, bool useCache = true);
        Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, int? skip, int? take, bool useCache = true);
    }
}