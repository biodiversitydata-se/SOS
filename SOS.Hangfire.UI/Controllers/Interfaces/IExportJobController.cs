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
        /// Run export all job
        /// </summary>
        /// <returns></returns>
        IActionResult RunDarwinCoreExportJob();

        /// <summary>
        /// Run export DOI job
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IActionResult RunDOIExportJob(AdvancedFilter filter);

        /// <summary>
        /// Schedule daily export all job
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyDarwinCoreExportJob(int hour, int minute);

        /// <summary>
        /// Schedule daily DOI all job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyDOIExportJob(AdvancedFilter filter, int hour, int minute);
    }
}
