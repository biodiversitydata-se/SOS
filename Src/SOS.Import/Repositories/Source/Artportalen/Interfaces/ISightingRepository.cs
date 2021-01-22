using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;
using SOS.Lib.Repositories.Interfaces;

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
        Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        ///     Get sightings for the specified sighting ids. Used for testing purpose for retrieving specific sightings from
        ///     Artportalen.
        ///     This method should be the same as GetChunkAsync(int startId, int maxRows), with
        ///     the difference that this method uses a list of sighting ids instead of (startId, maxRows).
        /// </summary>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds);

        /// <summary>
        ///     Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<Tuple<int, int>> GetIdSpanAsync();

        /// <summary>
        ///     Get last modified date for sightings
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> GetLastModifiedDateAsyc();

        /// <summary>
        /// Get list of id's of modified items
        /// </summary>
        /// <param name="modifiedSince"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> GetModifiedIdsAsync(DateTime modifiedSince, int limit);

        /// <summary>
        /// Get connections between project and sighting
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        Task<IEnumerable<(int SightingId, int ProjectId)>> GetSightingProjectIdsAsync(IEnumerable<int> sightingIds);

        /// <summary>
        /// True if live data base should be used
        /// </summary>
        bool Live { get; set; }

        /// <summary>
        /// Harvest protected observations
        /// </summary>
        ObservationType ObservationType { get; set; }
    }
}