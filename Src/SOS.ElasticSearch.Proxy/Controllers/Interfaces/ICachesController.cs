using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.ElasticSearch.Proxy.Controllers.Interfaces
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
    }
}