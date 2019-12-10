using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IDarwinCoreRepository : IProcessBaseRepository<DarwinCore<DynamicProperties>, ObjectId>
    {
        /// <summary>
        /// Create search index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        /// Toggle active instance
        /// </summary>
        /// <param name="start"></param>
        /// <param name="harvestInfo"></param>
        /// <returns></returns>
        Task<bool> ToggleInstanceAsync(DateTime start);

        /// <summary>
        /// Delete provider data
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task<bool> DeleteProviderDataAsync(DataProvider provider);
    }
}
