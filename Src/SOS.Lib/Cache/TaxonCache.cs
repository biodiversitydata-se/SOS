using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Taxon cache
    /// </summary>
    public class TaxonCache : CacheBase<int, Taxon>
    {
        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonRepository"></param>        
        /// <param name="logger"></param>
        public TaxonCache(ITaxonRepository taxonRepository, ILogger<CacheBase<int, Taxon>> logger) : base(taxonRepository, logger)
        {
            
        }
    }
}