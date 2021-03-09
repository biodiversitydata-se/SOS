using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Data provider cache
    /// </summary>
    public class DataProviderCache : CacheBase<int, DataProvider>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderRepository"></param>
        public DataProviderCache(IDataProviderRepository dataProviderRepository) : base(dataProviderRepository)
        {

        }
    }
}
