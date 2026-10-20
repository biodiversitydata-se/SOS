using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Services.Interfaces;

/// <summary>
///     Skogsskada data service interface
/// </summary>
public interface ISkogsskadaDataService
{
    /// <summary>
    ///  Service configuration
    /// </summary>
    SkogsskadaConfiguration Configuration { get; }

    /// <summary>
    /// Query data base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> QueryAsync<T>(string query, dynamic? parameters = null);
}