﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Area repository interface
    /// </summary>
    public interface IAreaRepository
    {
        /// <summary>
        ///     Get all areas
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AreaEntity>> GetAsync();
    }
}