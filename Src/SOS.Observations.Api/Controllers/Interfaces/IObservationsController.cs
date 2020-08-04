using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Sighting controller interface
    /// </summary>
    public interface IObservationsController
    {
        /// <summary>
        ///     Search for observations by the provided filter. All permitted values are either specified in the Field Mappings
        ///     object
        ///     retrievable from the Field Mappings endpoint or by the range of the underlying data type. All fields containing
        ///     the substring "Id" (but not exclusively) are mapped in the Field Mappings object.
        /// </summary>
        /// <param name="filter">Filter used to limit the search</param>
        /// <param name="skip">Start index of returned observations</param>
        /// <param name="take">End index of returned observations</param>
        /// <returns>List of matching observations</returns>
        /// <example>
        ///     Get all observations within 100m of provided point
        ///     "geometryFilter": {
        ///     "maxDistanceFromPoint": 100,
        ///     "geometry": {
        ///     "coordinates": [ 12.3456(lon), 78.9101112(lat) ],
        ///     "type": "Point"
        ///     },
        ///     "usePointAccuracy": false
        ///     }
        /// </example>
        Task<IActionResult> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        ///     Term dictionary are used for properties with multiple acceptable fixed values. E.g gender can have the values: male, female...
        ///     Term dictionary also describe the different possible query parameters available in searches.
        /// </summary>
        /// <returns>List of term dicionaries.</returns>
        Task<IActionResult> GetFieldMappingAsync();
    }
}