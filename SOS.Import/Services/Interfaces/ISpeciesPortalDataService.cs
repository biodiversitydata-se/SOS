using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Species portal data service interface
    /// </summary>
    public interface ISpeciesPortalDataService
    {
        /// <summary>
        /// Query data base
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null);
    }
}
