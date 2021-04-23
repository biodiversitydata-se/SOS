using System.Threading.Tasks;
using System.Xml.Linq;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// Area cache interface
    /// </summary>
    public interface IDataProviderCache : ICache<int, DataProvider>
    {
       /// <summary>
       /// Get eml data
       /// </summary>
       /// <param name="providerId"></param>
       /// <returns></returns>
        Task<XDocument> GetEmlAsync(int providerId);

       /// <summary>
       ///  Store eml data
       /// </summary>
       /// <param name="providerId"></param>
       /// <param name="eml"></param>
       /// <returns></returns>
        Task<bool> StoreEmlAsync(int providerId, XDocument eml);
    }
}
