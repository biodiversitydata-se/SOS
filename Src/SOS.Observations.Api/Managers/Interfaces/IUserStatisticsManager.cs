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
        Task<IEnumerable<UserStatisticsItem>> SpeciesCountSearchAsync();

        Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(int? skip, int? take);
    }
}