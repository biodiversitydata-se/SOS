using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Process job controller
    /// </summary>
    public interface IProcessJobController
    {
        /// <summary>
        /// Add daily process job
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyProcessJob(int sources, bool cleanStart, bool copyFromActiveOnFail, bool toggleInstanceOnSuccess, int hour, int minute);

        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <returns></returns>
        IActionResult RunProcessJob(int sources, bool cleanStart, bool copyFromActiveOnFail, bool toggleInstanceOnSuccess);

        /// <summary>
        /// Add daily process taxa job.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyProcessTaxaJob([FromQuery] int hour, [FromQuery] int minute);
        
        /// <summary>
        /// Run process taxa job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessTaxaJob();

        /// <summary>
        /// Run copy field mapping job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunCopyFieldMappingJob();

        /// <summary>
        /// Run copy areas job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunCopyAreasJob();
    }
}
