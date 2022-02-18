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
        public ProcessedConfigurationCache(IProcessedConfigurationRepository processedConfigurationRepository) : base(processedConfigurationRepository)
        {

        }
    }
}