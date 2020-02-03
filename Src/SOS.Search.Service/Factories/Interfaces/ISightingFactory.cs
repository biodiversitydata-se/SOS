﻿using System.Threading.Tasks;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Enum;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder);
    }
}
