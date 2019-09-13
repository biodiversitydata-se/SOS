using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Batch.Import.AP.Entities;

namespace SOS.Batch.Import.AP.Repositories.Source.Interfaces
{
    /// <summary>
    /// Sighting repository interface
    /// </summary>
    public interface ISightingRepository
    {
        /// <summary>
        /// Get chunk of sightings from Species Portal
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        /// Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<Tuple<int, int>> GetIdSpanAsync();

        /// <summary>
        /// Get all connections between project and sighting
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Tuple<int, int>>> GetProjectIdsAsync();
    }
}
