using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Taxon cache
    /// </summary>
    public class TaxonCache : CacheBase<int, Taxon>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonRepository"></param>
        public TaxonCache(ITaxonRepository taxonRepository, IMemoryCache memoryCache, ILogger<CacheBase<int, Taxon>> logger) : base(taxonRepository, memoryCache, logger)
        {
            CacheDuration = TimeSpan.FromMinutes(10);
        }
    }
}