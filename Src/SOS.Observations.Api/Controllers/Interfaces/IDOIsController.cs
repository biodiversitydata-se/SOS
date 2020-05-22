using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    /// Export job controller
    /// </summary>
    public interface IDOIsController
    {
        /// <summary>
        /// Get DOIs
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> GetDOIsAsync(int skip = 0, int take = 100);

        /// <summary>
        /// Get a DOI file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IActionResult GetDOIFileUrl(Guid id);

        /// <summary>
        /// Create a DOI based on provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>Object with these properties: fileName, jobId</returns>
        Task<IActionResult> RunCreateDOIJobAsync([FromBody] ExportFilter filter);
    }
}
