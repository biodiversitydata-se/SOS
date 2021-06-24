using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Log;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProtectedLogRepository : IMongoDbProcessedRepositoryBase<ProtectedLog, ObjectId>
    {
        /// <summary>
        ///     Create index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        /// Search log
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<IEnumerable<ProtectedLog>> SearchAsync(DateTime from, DateTime to);
    }
}