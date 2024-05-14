using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Taxon list cache
    /// </summary>
    public class TaxonListCache : CacheBase<int, TaxonList>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonListRepository"></param>
        public TaxonListCache(ITaxonListRepository taxonListRepository, ILogger<TaxonListCache> logger) : base(taxonListRepository, logger)
        {

        }
    }
}