using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Observations.Api.Managers.Interfaces
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
    }
}