using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Checklists controller controller interface.
    /// </summary>
    public interface IChecklistsController
    {
        /// <summary>
        /// Calculate taxon trend
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> CalculateTrendAsync(CalculateTrendFilterDto filter);

        /// <summary>
        /// Get a checklist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IActionResult> GetChecklistByIdAsync(
            string id);

        /// <summary>
        /// Get a checklist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IActionResult> GetChecklistByIdInternalAsync(
            string id);
    }
}