using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.ObservationsDatabase;

namespace SOS.Harvest.Repositories.Source.ObservationsDatabase.Interfaces
{
    /// <summary>
    ///     Observation repository interface
    /// </summary>
    public interface IObservationDatabaseRepository
    {
        /// <summary>
        /// Get chunk of sightings from observations db
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        Task<IEnumerable<ObservationEntity>> GetChunkAsync(int startId, int maxRows);

        /// <summary>
        ///     Get min and max id
        /// </summary>
        /// <returns></returns>
        Task<(int minId, int maxId)> GetIdSpanAsync();

        /// <summary>
        /// Get last modified date
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> GetLastModifiedDateAsyc();
    }
}