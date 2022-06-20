using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Observations.Api.Dtos.Filter;

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

        /// <summary>
        /// Search for locations
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sensitiveObservations"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <returns></returns>
        Task<IActionResult> SearchAsync(GeographicsFilterDto filter, int skip, int take, bool sensitiveObservations = false, int? roleId = null,
            string authorizationApplicationIdentifier = null);
    }
}