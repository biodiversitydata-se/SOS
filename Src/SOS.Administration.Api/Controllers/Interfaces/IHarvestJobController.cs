using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
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
        IActionResult AddDailyClamPortalHarvestJob(int hour, int minute);

        /// <summary>
        /// Run clam/tree portal sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunClamPortalHarvestJob();

        /// <summary>
        /// Add daily harvest of geo data
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyGeoAreasHarvestJob(int hour, int minute);

        /// <summary>
        /// Run geo data harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunGeoAreasHarvestJob();

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
        /// Add daily harvest of sightings from NORS
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyNorsHarvestJob(int hour, int minute);

        /// <summary>
        /// Run NORS sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunNorsHarvestJob();

        /// <summary>
        /// Add daily harvest of sightings from SERS
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailySersHarvestJob(int hour, int minute);

        /// <summary>
        /// Run SERS sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunSersHarvestJob();

        /// <summary>
        /// Add daily harvest of sightings from species data portal
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyArtportalenHarvestJob(int hour, int minute);

        /// <summary>
        /// Run Artportalen sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunArtportalenHarvestJob();

        /// <summary>
        /// Schedule daily taxon harvest job
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyTaxonHarvestJob(int hour, int minute);

        /// <summary>
        /// Run taxon harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunTaxonHarvestJob();

        /// <summary>
        /// Run import field mapping.
        /// </summary>
        /// <returns></returns>
        IActionResult RunImportFieldMappingJob();
    }
}
