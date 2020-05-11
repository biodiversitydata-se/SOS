using System.Threading.Tasks;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Enum;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Observation manager interface
    /// </summary>
    public interface IObservationManager
    {
        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder);
    }
}
