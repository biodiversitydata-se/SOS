using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// Area repository
    /// </summary>
    public interface IAreaRepository : IBaseRepository<Area, int>
    {
        /// <summary>
        /// Get info on a single area
        /// </summary>
        /// <param name="areaId">Id of area</param>
        /// <returns></returns>
        public Task<Area> GetAreaAsync(int areaId);

        /// <summary>
        /// Get all the areas, paged
        /// </summary>
        /// <param name="areaType">Skip this many</param>
        /// <param name="skip">Skip this many</param>
        /// <param name="take">Take this many areas</param>
        /// <returns></returns>
        public Task<PagedResult<Area>> GetAreasAsync(AreaType areaType, string nameFilter, int skip, int take);
    }
}
