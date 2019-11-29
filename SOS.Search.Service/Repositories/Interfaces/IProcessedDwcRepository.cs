using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Search.Service.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedDarwinCoreRepository : IBaseRepository<DarwinCore<DynamicProperties>, ObjectId>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int taxonId, int skip, int take);

        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="fields"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> GetChunkAsync(int taxonId, IEnumerable<string> fields, int skip, int take);
    }
}
