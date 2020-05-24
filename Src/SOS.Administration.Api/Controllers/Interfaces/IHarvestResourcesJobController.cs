using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Resource harvest job controller
    /// </summary>
    public interface IHarvestResourcesJobController
    {
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