using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     Vocabulary manager.
    /// </summary>
    public interface IProjectManager
    {
        /// <summary>
        ///     Get projects.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectInfo>> GetAllAsync();

        /// <summary>
        /// Get projects by id
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectInfo>> GetAsync(IEnumerable<int> projectIds);
    }
}