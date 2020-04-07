using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    /// Sighting controller interface
    /// </summary>
    public interface IObservationController
    {
        /// <summary>
        /// Search for observations by provided filter. 
        /// </summary>
        /// <param name="filter">Filter used to limit the search</param>
        /// <param name="skip">Start index of returned observations</param>
        /// <param name="take">End index of returned observations</param>
        /// <returns>List of matching observations</returns>
        Task<IActionResult> GetChunkAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Field mappings are used for properties with multiple acceptable fixed values. E.g gender can have the values: male, female...
        /// </summary>
        /// <returns>List of filed mappings</returns>
        Task<IActionResult> GetFieldMappingAsync();
    }
}
