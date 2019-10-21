using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.DarwinCore;

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
        Task<IEnumerable<DarwinCore<string>>> GetChunkAsync(int taxonId, int skip, int take);

        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> GetChunkAsync(int taxonId, IEnumerable<string> fields, int skip, int take);
    }
}
