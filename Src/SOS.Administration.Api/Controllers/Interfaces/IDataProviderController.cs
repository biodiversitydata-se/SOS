using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    /// Interface for data provider controller
    /// </summary>
    public interface IDataProviderController
    {
        /// <summary>
        /// Initialize the DataProvider collection with default data providers.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> CreateDefaultDataprovidersAsync();
    }
}