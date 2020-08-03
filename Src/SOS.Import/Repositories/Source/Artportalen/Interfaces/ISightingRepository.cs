using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Sighting repository interface
    /// </summary>
    public interface ISightingRepository
    {
        /// <summary>
        /// Get chunk of sightings from Artportalen
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <param name="liveData"></param>
        /// <returns></returns>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows, bool liveData);

        /// <summary>
        ///     Get sightings for specified sighting ids.
        /// </summary>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds);

        /// <summary>
        ///     Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<Tuple<int, int>> GetIdSpanAsync();

        /// <summary>
        /// Get highest id from live database
        /// </summary>
        /// <returns></returns>
        Task<int> GetMaxIdLiveAsync();

        /// <summary>
        ///     Get last modified date for sightings
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> GetLastModifiedDateAsyc();

        /// <summary>
        ///     Get all connections between project and sighting
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<(int SightingId, int ProjectId)>> GetProjectIdsAsync();
    }
}