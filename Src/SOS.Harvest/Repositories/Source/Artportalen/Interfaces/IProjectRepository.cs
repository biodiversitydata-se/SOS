using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Project repository interface
    /// </summary>
    public interface IProjectRepository : IBaseRepository<IProjectRepository>
    {
        /// <summary>
        ///     Get all projects
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectEntity>> GetProjectsAsync();
        /// <summary>
        ///     Get a project
        /// </summary>
        /// <returns></returns>
        Task<ProjectEntity> GetProjectAsync(int projectId);

        /// <summary>
        /// Get all project parameters for passed sightings
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectParameterEntity>> GetSightingProjectParametersAsync(IEnumerable<int> sightingIds);
    }
}