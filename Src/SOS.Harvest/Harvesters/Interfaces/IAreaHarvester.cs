﻿using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    /// <summary>
    ///     Area harvester
    /// </summary>
    public interface IAreaHarvester
    {
        /// <summary>
        ///     Aggregate all areas
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAreasAsync();
    }
}