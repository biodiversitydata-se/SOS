using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    ///     Artportalen data service interface
    /// </summary>
    public interface IArtportalenDataService
    {
        /// <summary>
        ///  Service configuration
        /// </summary>
        ArtportalenConfiguration Configuration { get; }

        /// <summary>
        /// Query data base
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic parameters = null, bool live = false);
    }
}