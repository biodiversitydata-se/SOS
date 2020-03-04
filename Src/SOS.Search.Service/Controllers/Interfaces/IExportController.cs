using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Search.Service.Controllers.Interfaces
{
    /// <summary>
    /// Export job controller
    /// </summary>
    public interface IExportController
    {
        /// <summary>
        /// Run export DOI job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        IActionResult RunExportJob(ExportFilter filter, string email);

        /// <summary>
        /// Get status of export job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        IActionResult GetExportStatus(string jobId);
    }
}
