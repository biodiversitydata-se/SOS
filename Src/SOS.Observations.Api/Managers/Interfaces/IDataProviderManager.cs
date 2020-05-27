using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Observations.Api.Dtos;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Data provider manager.
    /// </summary>
    public interface IDataProviderManager
    {
        /// <summary>
        /// Get data providers
        /// </summary>
        /// <param name="includeInactive">If true also inactive data providers will be included.</param>
        /// <returns></returns>
        Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive);
    }
}