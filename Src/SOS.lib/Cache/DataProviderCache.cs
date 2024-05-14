using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public DataProviderCache(IDataProviderRepository dataProviderRepository, ILogger<DataProviderCache> logger) : base(dataProviderRepository, logger)
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
