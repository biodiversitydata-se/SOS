using Microsoft.AspNetCore.Mvc;

namespace SOS.Hangfire.UI.Controllers.Interfaces
{
    /// <summary>
    /// Import job controller
    /// </summary>
    public interface IHarvestJobController
    {
        /// <summary>
        /// Add daily harvest of sightings from clam/tree portal
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyClamTreePortalHarvestJob(int hour, int minute);

        /// <summary>
        /// Run clam/tree portal sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunClamTreePortalHarvestJob();

        /// <summary>
        /// Add daily harvest of sightings from KUL
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyKulHarvestJob(int hour, int minute);

        /// <summary>
        /// Run KUL sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunKulHarvestJob();

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
