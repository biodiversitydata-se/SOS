using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Import job controller
    /// </summary>
    public interface IHarvestAndProcessJobController
    {
        /// <summary>
        /// Schedule harvest and process job
        /// </summary>
        /// <param name="harvestSources"></param>
        /// <param name="processSources"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyObservationHarvestAndProcessJob(int harvestSources, int processSources, int hour,int minute);

       /// <summary>
       /// Run harvest and process job
       /// </summary>
       /// <param name="harvestSources"></param>
       /// <param name="processSources"></param>
       /// <returns></returns>
        IActionResult RunObservationHarvestAndProcessJob(int harvestSources, int processSources);
    }
}
