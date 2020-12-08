using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Area controller interface
    /// </summary>
    public interface IAreasController
    {
        /// <summary>
        ///     Gets all the areas used for searching via areaId in the /search call
        /// </summary>
        /// <param name="areaTypes">Filter used to limit number of areas returned</param>
        /// <param name="searchString">Filter used to limit number of areas returned</param>
        /// <param name="skip">Start index of returned areas</param>
        /// <param name="take">Number of areas to return</param>
        /// <returns>List of Areas</returns>
        Task<IActionResult> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString, int skip = 0,
            int take = 100);

        /// <summary>
        /// Exports an area including its polygon
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        Task<IActionResult> ExportAreaAsync(AreaType areaType, string feature);
    }
   
}