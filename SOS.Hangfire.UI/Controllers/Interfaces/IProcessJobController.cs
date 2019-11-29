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

        IActionResult RunKulProcessJob();
    }
}
