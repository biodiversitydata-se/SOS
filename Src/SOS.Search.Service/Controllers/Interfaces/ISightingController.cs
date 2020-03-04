﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Search;

namespace SOS.Search.Service.Controllers.Interfaces
{
    /// <summary>
    /// Sighting controller interface
    /// </summary>
    public interface ISightingController
    {
        /// <summary>
        /// Get chunk of sightings 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> GetChunkAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Get field mappings.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetFieldMappingAsync();
    }
}
