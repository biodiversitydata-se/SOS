﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Export job controller
    /// </summary>
    public interface IExportsController
    {
        /// <summary>
        /// Get list of available export files
        /// </summary>
        /// <returns></returns>
        IActionResult GetExportFiles();

        /// <summary>
        /// Get url to export file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IActionResult GetExportFileUrl(string fileName);

        /// <summary>
        ///     Request of a Darwin Core Archive file with observations based on provided filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> RunExportAndSendJob(ExportFilterDto filter);
    }
}