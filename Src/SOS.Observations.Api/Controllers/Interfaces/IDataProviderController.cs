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
        /// <param name="includeProvidersWithNoObservations">If false, data providers with no observations are excluded from the result.</param>
        /// <returns>List of data providers.</returns>
        Task<IActionResult> GetDataProvidersAsync(string cultureCode = "en-GB", bool includeProvidersWithNoObservations = false);

        /// <summary>
        /// Get latest modified date for a data provider.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<IActionResult> GetLastModifiedDateByIdAsync(int providerId);

        /// <summary>
        /// Get provider EML file
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<IActionResult> GetEMLAsync(int providerId);

        /// <summary>
        /// Make dataproviders health check
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> HealthCheckAsync();
    }
}