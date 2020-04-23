using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get information about a single area
        /// </summary>
        /// <returns></returns>
        Task<Area> GetAreaAsync(int areaId);
        /// <summary>
        /// Get all the areas
        /// </summary>
        /// <param name="skip">Skip this many</param>
        /// <param name="take">Limit on how many to return</param>
        /// <returns></returns>
        Task<PagedAreas> GetAreasAsync(int skip, int take);
    }
}
