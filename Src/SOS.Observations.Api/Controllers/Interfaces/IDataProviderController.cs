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
        ///  Get all data providers.
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns>List of data providers.</returns>
        Task<IActionResult> GetDataProvidersAsync(string cultureCode = "en-GB");

        /// <summary>
        /// Get latest modified date for a data provider.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<IActionResult> GetLatestModifiedDateForProviderAsync(int providerId);

    }
}