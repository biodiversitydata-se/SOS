using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Batch.Import.AP.Entities;

namespace SOS.Batch.Import.AP.Repositories.Source.Interfaces
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
