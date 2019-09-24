using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Search.Service.Models;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SightingAggregate>> GetChunkAsync(int taxonId, int skip, int take);
    }
}
