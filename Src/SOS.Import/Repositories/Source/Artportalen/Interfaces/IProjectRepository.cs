using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    /// Project repository interface
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectEntity>> GetProjectsAsync();

        /// <summary>
        /// Get all project parameters.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectParameterEntity>> GetProjectParametersAsync();
    }
}
