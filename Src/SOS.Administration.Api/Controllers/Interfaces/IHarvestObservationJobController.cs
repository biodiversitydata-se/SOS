using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Harvest observations job controller
    /// </summary>
    public interface IHarvestObservationJobController
    {
        /// <summary>
        ///     Run observations harvest for the specified data providers.
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <returns></returns>
        Task<IActionResult> RunObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers);

        /// <summary>
        ///     Add daily harvest for the specified data providers.
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        Task<IActionResult> AddObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers,
            [FromQuery] int hour, [FromQuery] int minute);

        /// <summary>
        ///     Add daily harvest of sightings from clam/tree portal
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyClamPortalHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run clam/tree portal sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunClamPortalHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from Fish Data
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyFishDataHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run Fish Data sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunFishDataHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from KUL
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyKulHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run KUL sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunKulHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from MVM
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyMvmHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run MVM sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunMvmHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from NORS
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyNorsHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run NORS sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunNorsHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from SERS
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailySersHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run SERS sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunSersHarvestJob();

        /// <summary>
        ///     Add daily harvest of sightings from SHARK
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailySharkHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run SHARK sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunSharkHarvestJob();

        /// <summary>
        /// Add daily harvest of sightings from species data portal
        /// </summary>
        /// <param name="incrementalHarvest"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyArtportalenHarvestJob(bool incrementalHarvest, int hour, int minute);

        /// <summary>
        /// Run Artportalen sightings harvest
        /// </summary>
        /// <param name="incrementalHarvest"></param>
        /// <returns></returns>
        IActionResult RunArtportalenHarvestJob(bool incrementalHarvest);

        /// <summary>
        ///     Add daily harvest of sightings from Virtual Herbarium
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyVirtualHerbariumHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run Virtual Herbarium sightings harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunVirtualHerbariumHarvestJob();
    }
}