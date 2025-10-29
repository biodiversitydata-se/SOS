using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Data provider cache
    /// </summary>
    public class DataProviderCache : CacheBase<int, DataProvider>, IDataProviderCache
    {

        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderRepository"></param>        
        /// <param name="logger"></param>
        public DataProviderCache(IDataProviderRepository dataProviderRepository, ILogger<CacheBase<int, DataProvider>> logger) : base(dataProviderRepository, logger)
        {            
        }

        public async Task<IEnumerable<int>> GetDefaultIdsAsync()
        {
            var allProviders = await GetAllAsync();
            return allProviders?.Where(p => p.IsActive && p.IncludeInSearchByDefault).Select(p => p.Id).ToArray(); 
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
