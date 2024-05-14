using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Resource.Interfaces;

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
        public ProcessedConfigurationCache(IProcessedConfigurationRepository processedConfigurationRepository, ILogger<ProcessedConfigurationCache> logger) : base(processedConfigurationRepository, logger)
        {

        }
    }
}