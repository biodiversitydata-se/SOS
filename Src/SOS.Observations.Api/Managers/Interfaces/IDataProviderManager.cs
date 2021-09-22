using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SOS.Observations.Api.Dtos;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Data provider manager.
    /// </summary>
    public interface IDataProviderManager
    {
        /// <summary>
        /// Get data providers
        /// </summary>
        /// <param name="includeInactive"></param>
        /// <param name="cultureCode"></param>
        /// <param name="includeProvidersWithNoObservations"></param>
        /// <returns></returns>
        Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode, bool includeProvidersWithNoObservations = true);

        /// <summary>
        /// Get provider EML file
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<byte[]> GetEmlFileAsync(int providerId);
    }
}