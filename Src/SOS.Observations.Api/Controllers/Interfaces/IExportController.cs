using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    /// Export job controller
    /// </summary>
    public interface IExportsController
    {
        /// <summary>
        /// Request of a Darwin Core Archive file with observations based on provided filter 
        /// </summary>
        /// <param name="filter">Filter criteria used to limit observations returned</param>
        /// <param name="email">Email address used to inform you when the file is ready to pick up</param>
        /// <returns>Job id that can be used to see current status of the file creation process</returns>
        IActionResult RunExportJob(ExportFilter filter, string email);

        /// <summary>
        /// Get status of export job
        /// </summary>
        /// <param name="jobId">Job id returned when export was requested</param>
        /// <returns></returns>
        IActionResult GetExportStatus(string jobId);
    }
}
