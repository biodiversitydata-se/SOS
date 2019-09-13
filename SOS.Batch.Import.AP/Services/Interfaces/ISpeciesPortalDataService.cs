using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Batch.Import.AP.Services.Interfaces
{
    /// <summary>
    /// Species portal data service interface
    /// </summary>
    public interface ISpeciesPortalDataService
    {
        /// <summary>
        /// Query data base
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string query);
    }
}
