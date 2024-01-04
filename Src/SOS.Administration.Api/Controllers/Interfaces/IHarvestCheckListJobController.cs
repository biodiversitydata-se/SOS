﻿using Microsoft.AspNetCore.Mvc;
using SOS.Administration.Api.Models;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Harvest observations job controller
    /// </summary>
    public interface IHarvestChecklistJobController
    {
        /// <summary>
        /// Harvest observations from Darwin Core Archive file
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<IActionResult> RunDwcArchiveHarvestJob([FromForm] UploadDwcArchiveModelDto model);
    }
}