using SOS.Lib.Models.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Lib.Cache.Interfaces;

/// <summary>
/// Area cache interface
/// </summary>
public interface IDataProviderCache : ICache<int, DataProvider>
{
    /// <summary>
    /// Get Default providers id
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<int>> GetDefaultIdsAsync();

    /// <summary>
    /// Get a copy of EML data, cached per provider for up to one hour in this process.
    /// </summary>
    /// <param name="providerId"></param>
    /// <returns></returns>
    Task<XDocument> GetEmlAsync(int providerId);

    /// <summary>
    /// Store EML data and invalidate the cached document on success.
    /// </summary>
    /// <param name="providerId"></param>
    /// <param name="eml"></param>
    /// <returns></returns>
    Task<bool> StoreEmlAsync(int providerId, XDocument eml);
}
