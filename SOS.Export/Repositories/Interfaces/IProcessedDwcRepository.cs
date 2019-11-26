using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedDarwinCoreRepository : IBaseRepository<DarwinCore<DynamicProperties>, ObjectId>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int skip, int take);
    }
}
