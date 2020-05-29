using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Harvest and process job controller
    /// </summary>
    public interface IHarvestAndProcessJobController
    {
        /// <summary>
        ///     Schedule harvest and process job
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyObservationHarvestAndProcessJob(int hour, int minute);

        /// <summary>
        ///     Run harvest and process job
        /// </summary>
        /// <returns></returns>
        IActionResult RunObservationHarvestAndProcessJob();

        /// <summary>
        ///     Run harvest and process job
        /// </summary>
        /// <param name="harvestDataProviderIdOrIdentifiers"></param>
        /// <param name="processDataProviderIdOrIdentifiers"></param>
        /// <returns></returns>
        Task<IActionResult> RunObservationHarvestAndProcessJob(
            List<string> harvestDataProviderIdOrIdentifiers,
            List<string> processDataProviderIdOrIdentifiers);
    }
}