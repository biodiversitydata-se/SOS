using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading.Tasks;

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