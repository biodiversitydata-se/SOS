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
        /// <returns></returns>
        Task<IActionResult> RunProcessJob();

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

        /// <summary>
        /// Run taxon area aggregation job
        /// </summary>
        /// <returns></returns>
        IActionResult RunProcessTaxonAreaAggregationJob();

        /// <summary>
        /// Schedule taxon area aggregation job
        /// </summary>
        /// <param name="runIntervalInMinutes"></param>
        /// <returns></returns>
        IActionResult ScheduleProcessTaxonAreaAggregationJob(byte runIntervalInHours);
    }
}