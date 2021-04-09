using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Area controller interface
    /// </summary>
    public interface IAreasController
    {
        /// <summary>
        ///     Search for areas (regions).
        /// </summary>
        /// <param name="areaTypes">Filter used to limit number of areas returned</param>
        /// <param name="searchString">Filter used to limit number of areas returned</param>
        /// <param name="skip">Start index of returned areas</param>
        /// <param name="take">Number of areas to return</param>
        /// <returns>List of areas</returns>
        Task<IActionResult> GetAreas(IEnumerable<AreaTypeDto> areaTypes, string searchString, int skip = 0,
            int take = 100);

        /// <summary>
        ///     Get an area as a zipped JSON file including its polygon.
        /// </summary>
        /// <param name="areaType">The area type.</param>
        /// <param name="featureId">The FeatureId.</param>
        /// <returns></returns>
        Task<IActionResult> GetExport(AreaTypeDto areaType, string featureId);
    }
}