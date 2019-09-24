using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Search.Service.Models;

namespace SOS.Search.Service.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISightingAggregateRepository : IAggregateRepository<SightingAggregate, int>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<SightingAggregate>> GetChunkAsync(int taxonId, int skip, int take);
    }
}
