using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Search.Service.Models;

namespace SOS.Search.Service.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedDarwinCoreRepository : IAggregateRepository<DarwinCore<DynamicProperties>, string>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int taxonId, int skip, int take);
    }
}
