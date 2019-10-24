using Microsoft.AspNetCore.Mvc;

namespace SOS.Hangfire.UI.Controllers.Interfaces
{
    /// <summary>
    /// Import job controller
    /// </summary>
    public interface IHarvestJobController
    {
        /// <summary>
        /// Add daily harvest of sightings from species data portal
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailySpeciesPortalHarvestJob(int hour, int minute);

        /// <summary>
        /// Run species portal sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunSpeciesPortalHarvestJob();
    }
}
