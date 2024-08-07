﻿using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Sighting repository interface
    /// </summary>
    public interface ISightingRepository : IBaseRepository<ISightingRepository>
    {
        /// <summary>
        /// Get chunk of sightings from Artportalen
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <param name="liveData"></param>
        /// <returns></returns>
        Task<IEnumerable<SightingEntity>?> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        ///     Get sightings for the specified sighting ids. Used for testing purpose for retrieving specific sightings from
        ///     Artportalen.
        ///     This method should be the same as GetChunkAsync(int startId, int maxRows), with
        ///     the difference that this method uses a list of sighting ids instead of (startId, maxRows).
        /// </summary>
        Task<IEnumerable<SightingEntity>?> GetChunkAsync(IEnumerable<int> sightingIds);

        /// <summary>
        /// Get list of sigthing id's deleted in AP
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> GetDeletedIdsAsync(DateTime from);

        /// <summary>
        /// Get id's of rejected sightings 
        /// </summary>
        /// <param name="modifiedSince"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> GetRejectedIdsAsync(DateTime modifiedSince);

        /// <summary>
        ///     Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<(int minId, int maxId)> GetIdSpanAsync();

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
        Task<IEnumerable<NewAndEditedSightingId>> GetModifiedIdsAsync(DateTime modifiedSince, int limit);

        /// <summary>
        /// Get connections between project and sighting
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        Task<IEnumerable<(int SightingId, int ProjectId)>> GetSightingProjectIdsAsync(IEnumerable<int> sightingIds);

        /// <summary>
        /// Get sighting and taxon id's for checklist
        /// </summary>
        /// <param name="checklistIds"></param>
        /// <returns></returns>
        Task<IDictionary<int, ICollection<(int sightingId, int taxonId)>>> GetSightingsAndTaxonIdsForChecklistsAsync(
            IEnumerable<int> checklistIds);
    }
}