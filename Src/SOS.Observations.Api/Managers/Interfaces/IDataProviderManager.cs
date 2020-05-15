using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

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
        /// <returns></returns>
        Task<IEnumerable<DataProvider>> GetDataProvidersAsync();
    }
}