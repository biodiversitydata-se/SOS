using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     DataProvider controller interface
    /// </summary>
    public interface IDataProviderController
    {
        /// <summary>
        ///     Get all data providers.
        /// </summary>
        /// <returns>List of data providers.</returns>
        Task<IActionResult> GetDataProvidersAsync();

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<IActionResult> GetLatestModifiedDateForProviderAsync(int providerId);
    }
}