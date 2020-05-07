using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public interface IProcessedAreaRepository : IProcessBaseRepository<Area, int>
    {
        /// <summary>
        /// Get all areas, but skip getting the geometry field.
        /// </summary>
        /// <returns></returns>
        Task<List<AreaBase>> GetAllAreaBaseAsync();

        /// <summary>
        /// Create indexes
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();
    }
}
