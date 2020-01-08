using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedDarwinCoreRepository : IBaseRepository<DarwinCore<DynamicProperties>, ObjectId>
    {
        /// <summary>
        /// Get filtered chunk
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(AdvancedFilter filter, int skip, int take);

        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<DarwinCoreProject>> GetProjectParameters(int skip, int take);
    }
}
