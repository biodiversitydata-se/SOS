﻿using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search.Filters;

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
        IActionResult RunExportAndStoreJob([FromBody] SearchFilter filter, [FromQuery] string blobStorageContainer,
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
        IActionResult ScheduleDailyExportAndStoreJob(SearchFilter filter, string blobStorageContainer, string fileName,
            int hour, int minute);

        /// <summary>
        /// Make a DOI of a export file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IActionResult RunExportToDoi([FromRoute] string fileName);

        /// <summary>
        /// Schedule export file to DOI job
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        IActionResult ScheduleExportToDoi([FromRoute] string fileName, string cronExpression);
    }
}