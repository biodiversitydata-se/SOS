using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Observations controller interface.
    /// </summary>
    public interface ICheckListsController
    {
        /// <summary>
        /// Calculate taxon trend
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> CalculateTrendAsync(CalculateTrendFilterDto filter);

        /// <summary>
        /// Get a check list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IActionResult> GetCheckListAsync(
            string id);

    }
}