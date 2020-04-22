using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Process information manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get process information 
        /// </summary>
        /// <returns></returns>
        Task<Area> GetAreaAsync(int areaId);
        Task<PagedAreas> GetAreasAsync(int skip, int take);
    }
}
