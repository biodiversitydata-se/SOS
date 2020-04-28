using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    /// Sighting controller interface
    /// </summary>
    public interface IObservationController
    {
        /// <summary>
        /// Search for observations by the provided filter. All permitted values are either specified in the Field Mappings object
        /// retrievable from the Field Mappings endpoint or by the range of the underlying data type. All fields containing
        /// the substring "Id" (but not exclusively) are mapped in the Field Mappings object.
        /// </summary>
        /// <param name="filter">Filter used to limit the search</param>
        /// <param name="skip">Start index of returned observations</param>
        /// <param name="take">End index of returned observations</param>
        /// <returns>List of matching observations</returns>
        /// <example>
        /// Get all observations within 100m of provided point
        /// "geometryFilter": {
        /// "maxDistanceFromPoint": 100,
        /// "geometry": {
        ///         "coordinates": [ 12.3456(lon), 78.9101112(lat) ],
        ///         "type": "Point"
        ///     },
        ///     "usePointAccuracy": false
        /// }
        /// </example>
        Task<IActionResult> GetChunkAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Field Mappings are used for properties with multiple acceptable fixed values but limited by other contraints then permitted by
        /// the underlying data type. E.g gender can have the values: male, female...
        /// 
        /// Field Mappings also describe the different possible query parameters available in searches.
        /// </summary>
        /// <returns>List of Field Mappings</returns>
        Task<IActionResult> GetFieldMappingAsync();

        /// <summary>
        /// Gets all the areas used for searching via areaId in the /search call                
        /// </summary>
        /// <param name="areaType">Filter used to limit number of areas returned</param>
        /// <param name="searchString">Filter used to limit number of areas returned</param>
        /// <param name="skip">Start index of returned areas</param>
        /// <param name="take">End index of returned areas</param>
        /// <returns>List of Areas</returns>
        Task<IActionResult> GetAreasAsync(AreaType areaType, string searchString, int skip = 0, int take = 100);
    }
}
