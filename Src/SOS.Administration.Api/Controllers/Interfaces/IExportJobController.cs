using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Export job controller
    /// </summary>
    public interface IExportJobController
    {
        /// <summary>
        /// Run export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        IActionResult RunDarwinCoreExportJob(ExportFilter filter, string email);

        /// <summary>
        /// Schedule daily export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyDarwinCoreExportJob(ExportFilter filter, string email, int hour, int minute);
    }
}
