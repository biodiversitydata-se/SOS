using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IExportsController
    {
        /// <summary>
        ///     Request of a Darwin Core Archive file with observations based on provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        IActionResult RunExportAndSendJob([FromBody] ExportFilter filter, [FromQuery] string email);

        /// <summary>
        ///     Get status of export job
        /// </summary>
        /// <param name="jobId">Job id returned when export was requested</param>
        /// <returns></returns>
        IActionResult GetExportStatus(string jobId);
    }
}