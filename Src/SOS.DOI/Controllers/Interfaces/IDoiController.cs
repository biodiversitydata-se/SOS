﻿using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using System.Threading.Tasks;

namespace SOS.DOI.Controllers.Interfaces
{
    public interface IDoiController
    {
        /// <summary>
        /// Get batch of DOI's
        /// </summary>
        /// <param name="take"></param>
        /// <param name="page"></param>
        /// <param name="orderBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IActionResult> GetBatchAsync(int take, int page, string orderBy, SearchSortOrder sortOrder);

        /// <summary>
        ///  Get DOI meta data
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        Task<IActionResult> GetMetadata(string prefix, string suffix);

        /// <summary>
        ///  Get DOI download URL
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        IActionResult GetDOIFileUrl(string prefix, string suffix);

        /// <summary>
        /// Search for DOI's
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        Task<IActionResult> SearchMetadata([FromQuery] string searchFor);
    }
}
