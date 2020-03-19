﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Entities;
using SOS.Import.Entities.Artportalen;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    /// Sighting repository interface
    /// </summary>
    public interface ISightingRepository
    {
        /// <summary>
        /// Get chunk of sightings from Artportalen
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        /// Get sightings for specified sighting ids.
        /// </summary>
        Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds);

        /// <summary>
        /// Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<Tuple<int, int>> GetIdSpanAsync();

        /// <summary>
        /// Get all connections between project and sighting
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<(int SightingId, int ProjectId)>> GetProjectIdsAsync();
    }
}
