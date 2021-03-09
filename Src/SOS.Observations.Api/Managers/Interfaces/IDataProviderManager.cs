using System.Collections.Generic;
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
        /// <returns></returns>
        Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode);
    }
}