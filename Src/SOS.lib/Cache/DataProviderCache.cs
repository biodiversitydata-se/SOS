using System.Threading.Tasks;
using System.Xml.Linq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Data provider cache
    /// </summary>
    public class DataProviderCache : CacheBase<int, DataProvider>, IDataProviderCache
    {
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderRepository"></param>
        public DataProviderCache(IDataProviderRepository dataProviderRepository) : base(dataProviderRepository)
        {

        }

        /// <inheritdoc />
        public async Task<XDocument> GetEmlAsync(int providerId)
        {
            return await ((IDataProviderRepository)Repository).GetEmlAsync(providerId);
        }

        /// <inheritdoc />
        public async Task<bool> StoreEmlAsync(int providerId, XDocument eml)
        {
            return await ((IDataProviderRepository)Repository).StoreEmlAsync(providerId, eml);
        }
    }
}
