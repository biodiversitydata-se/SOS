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
        /// <param name="forceOverwriteIfCollectionExist">If the DataProvider collection already exists, set forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.</param>
        /// <returns></returns>
        Task<IActionResult> CreateDefaultDataprovidersAsync(bool forceOverwriteIfCollectionExist = false);
    }
}