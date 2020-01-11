using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
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
