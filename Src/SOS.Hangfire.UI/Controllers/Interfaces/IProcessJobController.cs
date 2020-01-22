using Microsoft.AspNetCore.Mvc;

namespace SOS.Hangfire.UI.Controllers.Interfaces
{
    /// <summary>
    /// Process job controller
    /// </summary>
    public interface IProcessJobController
    {
        /// <summary>
        /// Add daily process job
        /// </summary>
        /// <param name="sources">Bit flag. 1-Species Portal</param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyProcessJob(int sources, bool toggleInstanceOnSuccess, int hour, int minute);

        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="sources">Bit flag. 1-Species Portal</param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <returns></returns>
        IActionResult RunProcessJob(int sources, bool toggleInstanceOnSuccess);

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
        /// Run KUL process job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunKulProcessJob();
    }
}
