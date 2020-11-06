﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     DataProvider controller interface
    /// </summary>
    public interface IDataProviderController
    {
        /// <summary>
        ///     Gets all data providers.
        /// </summary>
        /// <returns>List of data providers</returns>
        Task<IActionResult> GetDataProvidersAsync();

        /// <summary>
        ///    Gets all active data providers.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> GetActiveDataProvidersAsync();
    }
}