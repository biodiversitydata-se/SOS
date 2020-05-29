using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Process job controller
    /// </summary>
    public interface IProcessJobController
    {
        /// <summary>
        ///     Run copy areas job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessAreasJob();

        /// <summary>
        ///     Add daily process job
        /// </summary>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyProcessJob(bool cleanStart, bool copyFromActiveOnFail, bool toggleInstanceOnSuccess,
            int hour, int minute);

        /// <summary>
        ///     Run process job
        /// </summary>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <returns></returns>
        Task<IActionResult> RunProcessJob(bool cleanStart, bool copyFromActiveOnFail, bool toggleInstanceOnSuccess);

        /// <summary>
        ///     Run process job for selected data providers
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <returns></returns>
        Task<IActionResult> RunProcessJob(List<string> dataProviderIdOrIdentifiers, bool cleanStart = true,
            bool copyFromActiveOnFail = false, bool toggleInstanceOnSuccess = true);

        /// <summary>
        ///     Add daily process taxa job.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyProcessTaxaJob([FromQuery] int hour, [FromQuery] int minute);

        /// <summary>
        ///     Run process taxa job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessTaxaJob();

        /// <summary>
        ///     Run copy field mapping job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunCopyFieldMappingJob();
    }
}