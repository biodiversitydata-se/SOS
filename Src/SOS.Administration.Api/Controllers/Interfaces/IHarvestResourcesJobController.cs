using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Resource harvest job controller
    /// </summary>
    public interface IHarvestResourcesJobController
    {
        /// <summary>
        ///     Add daily harvest of geo data
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyAreasHarvestJob(int hour, int minute);

        /// <summary>
        ///     Run geo data harvest
        /// </summary>
        /// <returns></returns>
        IActionResult RunAreasHarvestJob();

        /// <summary>
        ///     Run import field mapping.
        /// </summary>
        /// <returns></returns>
        IActionResult RunImportVocabulariesJob();
    }
}