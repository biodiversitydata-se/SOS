using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedLocationRepository : IProcessRepositoryBase<Observation, string>
    {
        /// <summary>
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds);

        /// <summary>
        /// Search for locations
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<LocationSearchResult>> SearchAsync(SearchFilter filter, int skip,
            int take);
    }
}