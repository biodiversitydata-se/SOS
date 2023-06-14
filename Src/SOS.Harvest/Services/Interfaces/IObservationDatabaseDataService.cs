using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services.Interfaces
{
    /// <summary>
    ///     Artportalen data service interface
    /// </summary>
    public interface IObservationDatabaseDataService
    {
        /// <summary>
        ///  Service configuration
        /// </summary>
        ObservationDatabaseConfiguration Configuration { get; }

        /// <summary>
        /// Query data base
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic? parameters = null);
    }
}