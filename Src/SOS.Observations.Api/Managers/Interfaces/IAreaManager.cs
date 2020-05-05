﻿using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Models.Area;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get information about a single area, used for internal calls
        /// </summary>
        /// <returns></returns>
        Task<Area> GetAreaInternalAsync(int areaId);

        /// <summary>
        /// Get zipped json bytes with an area json file
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetZipppedAreaAsync(int areaId);

        /// <summary>
        /// Get areas matching provided filter
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="searchString"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<ExternalSimpleArea>> GetAreasAsync(AreaType areaType, string searchString, int skip, int take);
    }
}
