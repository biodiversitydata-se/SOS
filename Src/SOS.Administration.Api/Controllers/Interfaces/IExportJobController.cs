using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IExportJobController
    {
        /// <summary>
        ///     Run export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="blobStorageContainer"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IActionResult RunExportAndStoreJob([FromBody] ExportFilter filter, [FromQuery] string blobStorageContainer,
            [FromQuery] string fileName);

        /// <summary>
        ///     Schedule daily export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="blobStorageContainer"></param>
        /// <param name="fileName"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult ScheduleDailyExportAndStoreJob(ExportFilter filter, string blobStorageContainer, string fileName,
            int hour, int minute);
    }
}