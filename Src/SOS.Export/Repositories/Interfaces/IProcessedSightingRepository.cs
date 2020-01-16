using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedSightingRepository : IBaseRepository<ProcessedSighting, ObjectId>
    {
        /// <summary>
        /// Get filtered chunk
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedSighting>> GetChunkAsync(AdvancedFilter filter, int skip, int take);

        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedProject>> GetProjectParameters(AdvancedFilter filter, int skip, int take);
    }
}
