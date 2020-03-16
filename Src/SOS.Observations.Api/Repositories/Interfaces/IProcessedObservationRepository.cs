using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, ObjectId>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take);
    }
}
