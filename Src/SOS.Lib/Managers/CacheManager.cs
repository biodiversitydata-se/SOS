using Microsoft.Extensions.Logging;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SOS.Lib.Managers
{
    /// <inheritdoc />
    public class CacheManager : ICacheManager
    {
        private readonly SosApiConfiguration _sosApiConfiguration;
        private readonly ILogger<CacheManager> _logger;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ICache<int, ProjectInfo> _projectCache;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        private readonly IAreaCache _areaCache;
        private readonly ICache<int, TaxonList> _taxonListCache;

        private async Task<bool> ClearAsync(HttpClient client, string requestUri)
        {
            try
            {
                var response = await client.DeleteAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Failed to clear cache ({requestUri})");
                    return false;
                }

                _logger.LogInformation($"Cache cleared ({requestUri})");


                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to clear cache ({requestUri})");
                return false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sosApiConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="projectCache"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="vocabularyCache"></param>
        /// <param name="areaCache"></param>
        /// <param name="taxonListCache"></param>
        /// <param name="logger"></param>
        public CacheManager(
            SosApiConfiguration sosApiConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ICache<int, ProjectInfo> projectCache,
            IDataProviderCache dataProviderCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            IAreaCache areaCache,
            ICache<int, TaxonList> taxonListCache,
            ILogger<CacheManager> logger)
        {
            _sosApiConfiguration = sosApiConfiguration ?? throw new ArgumentNullException(nameof(sosApiConfiguration));
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            _projectCache = projectCache ?? throw new ArgumentNullException(nameof(projectCache));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _vocabularyCache = vocabularyCache ?? throw new ArgumentNullException(nameof(vocabularyCache));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _taxonListCache = taxonListCache ?? throw new ArgumentNullException(nameof(taxonListCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> ClearAsync(Enums.Cache cache)
        {
            try
            {
                switch (cache)
                {
                    case Enums.Cache.ProcessedConfiguration:
                        await _processedConfigurationCache.ClearAsync();
                        break;
                    case Enums.Cache.TaxonLists:
                        await _taxonListCache.ClearAsync();
                        break;
                    case Enums.Cache.Area:
                        await _areaCache.ClearAsync();
                        break;
                    case Enums.Cache.Vocabulary:
                        await _vocabularyCache.ClearAsync();
                        break;
                    case Enums.Cache.Projects:
                        await _projectCache.ClearAsync();
                        break;
                    case Enums.Cache.DataProviders:
                        await _dataProviderCache.ClearAsync();
                        break;
                    default:
                        _logger.LogError($"CacheManager.ClearAsync({cache}) called. No handler implemented.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CacheManager.ClearAsync({cache}) error.");
            }

            //_logger.LogDebug($"CacheManager.ClearAsync({cache}) called. Distributed cache not implemented, but the local cache is cleared.");
            
            var success = true;
            var client = new HttpClient();
            if (_sosApiConfiguration?.ObservationsApiAddresses?.Any() ?? false)
            {
                foreach (var observationsApiAddress in _sosApiConfiguration.ObservationsApiAddresses)
                {
                    var requestUri = $"{observationsApiAddress}Caches/{cache}";
                    success = success && await ClearAsync(client, requestUri);
                }
            }

            if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.ElasticSearchProxyAddresses?.Any() ?? false))
            {
                foreach (var elasticSearchProxyAddress in _sosApiConfiguration.ElasticSearchProxyAddresses)
                {
                    var requestUri = $"{elasticSearchProxyAddress}Caches/{cache}";
                    success = success && await ClearAsync(client, requestUri);
                }
            }

            if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.AnalysisApiAddresses?.Any() ?? false))
            {
                foreach (var analysisApiAddress in _sosApiConfiguration.AnalysisApiAddresses)
                {
                    var requestUri = $"{analysisApiAddress}Caches/{cache}";
                    success = success && await ClearAsync(client, requestUri);
                }
            }

            if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.DataStewardshipApiAddresses?.Any() ?? false))
            {
                foreach (var dataStewardshipApiAddress in _sosApiConfiguration.DataStewardshipApiAddresses)
                {
                    var requestUri = $"{dataStewardshipApiAddress}Caches/{cache}";
                    success = success && await ClearAsync(client, requestUri);
                }
            }

            _logger.LogInformation($"CacheManager.ClearAsync({cache}) called.");
            return success;
        }
    }
}
