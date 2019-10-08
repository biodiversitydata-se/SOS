using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Search.Service.Controllers.Interfaces
{
    /// <summary>
    /// Sighting controller interface
    /// </summary>
    public interface ISightingController
    {
        /// <summary>
        /// Get chunk of sightings for a taxon
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> GetChunkAsync(int taxonId, int skip, int take);

        /// <summary>
        /// Get chunk of sightings for a taxon
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="fields"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> GetChunkAsync(int taxonId, IEnumerable<string> fields, int skip, int take);
    }
}
