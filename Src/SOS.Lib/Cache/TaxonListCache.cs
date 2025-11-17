using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache;

/// <summary>
/// Taxon list cache
/// </summary>
public class TaxonListCache : CacheBase<int, TaxonList>
{
    public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="taxonListRepository"></param>
    /// <param name="logger"></param>
    public TaxonListCache(ITaxonListRepository taxonListRepository,  ILogger<CacheBase<int, TaxonList>> logger) : base(taxonListRepository, logger)
    {
        
    }
}