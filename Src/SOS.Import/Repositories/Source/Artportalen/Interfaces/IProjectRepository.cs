using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Project repository interface
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        ///     Get all projects
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectEntity>> GetProjectsAsync(bool live = false);

        /// <summary>
        /// Get all project parameters for passed sightings
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectParameterEntity>> GetSightingProjectParametersAsync(IEnumerable<int> sightingIds, bool live = false);
    }
}