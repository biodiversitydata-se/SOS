using Microsoft.Extensions.Logging;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Shared;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ClearCacheJob : IClearCacheJob
    {
        private readonly IAreaCache _areaCache;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ICache<int, TaxonList> _taxonListCache;
        private readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        private readonly ILogger<ClearCacheJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaCache"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="taxonListCache"></param>
        /// <param name="vocabularyCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ClearCacheJob(
            IAreaCache areaCache,
            IDataProviderCache dataProviderCache,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ICache<int, TaxonList> taxonListCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            ILogger<ClearCacheJob> logger)
        {
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            _taxonListCache = taxonListCache ?? throw new ArgumentNullException(nameof(taxonListCache));
            _vocabularyCache = vocabularyCache ?? throw new ArgumentNullException(nameof(vocabularyCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task RunAsync(IEnumerable<Cache> caches)
        {
            if (!caches?.Any() ?? true)
            {
                return;
            }

            if (caches?.Any(c => c == Cache.Area) ?? false)
            {
                _areaCache.Clear();
            }

            if (caches?.Any(c => c == Cache.DataProviders) ?? false)
            {
                _dataProviderCache.Clear();
            }

            if (caches?.Any(c => c == Cache.ProcessedConfiguration) ?? false)
            {
                _processedConfigurationCache.Clear();
            }

            if (caches?.Any(c => c == Cache.TaxonLists) ?? false)
            {
                _taxonListCache.Clear();
            }

            if (caches?.Any(c => c == Cache.Vocabulary) ?? false)
            {
                _vocabularyCache.Clear();
            }

            _logger.LogInformation($"Cache/s cleared ({string.Join(',', caches!)})");
        }
    }
}