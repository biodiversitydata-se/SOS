using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Process job controller
    /// </summary>
    public interface IProcessJobController
    {
        /// <summary>
        ///     Run process job
        /// </summary>
        /// <param name="copyFromActiveOnFail"></param>
        /// <returns></returns>
        Task<IActionResult> RunProcessJob(bool copyFromActiveOnFail);

        /// <summary>
        /// Run process job for selected data providers
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task<IActionResult> RunProcessJob(List<string> dataProviderIdOrIdentifiers,
            [FromQuery] JobRunModes mode);

        /// <summary>
        ///     Run process taxa job.
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessTaxaJob();
    }
}