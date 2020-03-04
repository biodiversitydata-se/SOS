using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Hangfire.UI.Controllers.Interfaces
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
        /// <returns></returns>
        IActionResult RunDarwinCoreExportJob(ExportFilter filter);

        /// <summary>
        /// Schedule daily export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyDarwinCoreExportJob(ExportFilter filter, int hour, int minute);
    }
}
