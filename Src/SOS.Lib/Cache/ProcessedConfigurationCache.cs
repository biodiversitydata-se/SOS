using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Threading.Tasks;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Project cache.
    /// </summary>
    public class ProcessedConfigurationCache : CacheBase<string, ProcessedConfiguration>
    {
        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedConfigurationRepository"></param>
        /// <param name="logger"></param>
        public ProcessedConfigurationCache(IProcessedConfigurationRepository processedConfigurationRepository, ILogger<CacheBase<string, ProcessedConfiguration>> logger) 
            : base(processedConfigurationRepository, logger)
        {            
        }

        public override async Task<ProcessedConfiguration> GetAsync(string key)
        {
            ProcessedConfiguration result = await base.GetAsync(key);
            Logger.LogDebug($"ProcessedConfiguration retrieved from cache. Value={result?.ActiveInstance}");
            return result;
        }
    }
}