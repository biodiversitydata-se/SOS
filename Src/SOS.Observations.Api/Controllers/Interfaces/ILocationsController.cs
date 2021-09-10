using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Observations controller interface.
    /// </summary>
    public interface ILocationsController
    {
        /// <summary>
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
        Task<IActionResult> GetLocationsByIds(IEnumerable<string> locationIds);
    }
}