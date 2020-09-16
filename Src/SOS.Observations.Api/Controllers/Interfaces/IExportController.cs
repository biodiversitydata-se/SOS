using System.Threading.Tasks;
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
        /// Get list of available export files
        /// </summary>
        /// <returns></returns>
        IActionResult GetExportFiles();

        /// <summary>
        /// Get url to export file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IActionResult GetExportFileUrl(string fileName);

        /// <summary>
        ///     Get status of export job
        /// </summary>
        /// <param name="jobId">Job id returned when export was requested</param>
        /// <returns></returns>
        IActionResult GetExportStatus(string jobId);


        /// <summary>
        ///     Request of a Darwin Core Archive file with observations based on provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<IActionResult> RunExportAndSendJob(ExportFilter filter, string email);
    }
}