using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Caches controller interface
    /// </summary>
    public interface ICachesController
    {
        /// <summary>
        /// Clear a cache
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        IActionResult DeleteCache(Cache cache);

        /// <summary>
        /// Get a cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public Task<IActionResult> GetCache([FromRoute] Cache cache);
    }
}