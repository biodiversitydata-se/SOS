using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProjectInfoRepository : IRepositoryBase<ProjectInfo, int>
    {
        /// <summary>
        /// Create indexes
        /// </summary>
        /// <returns></returns>
        Task CreateIndexesAsync();

        /// <summary>
        /// Get projects
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectInfo>> GetAsync(string filter, int userId);
    }
}