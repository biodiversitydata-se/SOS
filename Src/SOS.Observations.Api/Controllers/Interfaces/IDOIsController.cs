using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IDOIsController
    {
        /// <summary>
        ///     Create a DOI based on provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>Object with these properties: fileName, jobId</returns>
        Task<IActionResult> RunCreateDOIJobAsync([FromBody] ExportFilter filter);
    }
}