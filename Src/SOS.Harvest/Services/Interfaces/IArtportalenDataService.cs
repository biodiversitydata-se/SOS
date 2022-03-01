using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services.Interfaces
{
    /// <summary>
    ///     Artportalen data service interface
    /// </summary>
    public interface IArtportalenDataService
    {
        /// <summary>
        /// Get name of backup database
        /// </summary>
        string BackUpDatabaseName { get; }

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