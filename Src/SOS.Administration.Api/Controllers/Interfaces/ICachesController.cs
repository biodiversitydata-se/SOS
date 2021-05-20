using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Cache controller
    /// </summary>
    public interface ICachesController
    {
        /// <summary>
        /// Clear requested cache
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        Task<IActionResult> ClearAsync(Cache cache);
    }
}