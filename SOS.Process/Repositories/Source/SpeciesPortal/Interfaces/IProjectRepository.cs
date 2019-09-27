using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Process.Entities;

namespace SOS.Process.Repositories.Source.SpeciesPortal.Interfaces
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
        Task<IEnumerable<ProjectEntity>> GetAsync();
    }
}
