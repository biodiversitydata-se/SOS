using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Project cache.
    /// </summary>
    public class ProcessedConfigurationCache : CacheBase<string, ProcessedConfiguration>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedConfigurationRepository"></param>
        public ProcessedConfigurationCache(IProcessedConfigurationRepository processedConfigurationRepository, IMemoryCache memoryCache, ILogger<CacheBase<string, ProcessedConfiguration>> logger) : base(processedConfigurationRepository, memoryCache, logger)
        {
            CacheDuration = TimeSpan.FromMinutes(1);
        }
    }
}