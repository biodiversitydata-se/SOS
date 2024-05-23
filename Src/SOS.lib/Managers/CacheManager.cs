using Microsoft.Extensions.Logging;
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
        /// <param name="logger"></param>
        public CacheManager(
            SosApiConfiguration sosApiConfiguration,
            ILogger<CacheManager> logger)
        {
            _sosApiConfiguration = sosApiConfiguration ?? throw new ArgumentNullException(nameof(sosApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> ClearAsync(Enums.Cache cache)
        {
            switch(cache)
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

            _logger.LogDebug($"CacheManager.ClearAsync({cache}) called. Distributed cache not implemented, but the local cache is cleared.");
            return true;
            //var success = true;
            //var client = new HttpClient();
            //if (_sosApiConfiguration?.ObservationsApiAddresses?.Any() ?? false)
            //{
            //    foreach (var observationsApiAddress in _sosApiConfiguration.ObservationsApiAddresses)
            //    {
            //        var requestUri = $"{observationsApiAddress}Caches/{cache}";
            //        success = success && await ClearAsync(client, requestUri);
            //    }
            //}

            //if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.ElasticSearchProxyAddresses?.Any() ?? false))
            //{
            //    foreach (var elasticSearchProxyAddress in _sosApiConfiguration.ElasticSearchProxyAddresses)
            //    {
            //        var requestUri = $"{elasticSearchProxyAddress}Caches/{cache}";
            //        success = success && await ClearAsync(client, requestUri);
            //    }
            //}

            //if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.AnalysisApiAddresses?.Any() ?? false))
            //{
            //    foreach (var analysisApiAddress in _sosApiConfiguration.AnalysisApiAddresses)
            //    {
            //        var requestUri = $"{analysisApiAddress}Caches/{cache}";
            //        success = success && await ClearAsync(client, requestUri);
            //    }
            //}

            //if (cache.Equals(Enums.Cache.ProcessedConfiguration) && (_sosApiConfiguration?.DataStewardshipApiAddresses?.Any() ?? false))
            //{
            //    foreach (var dataStewardshipApiAddress in _sosApiConfiguration.DataStewardshipApiAddresses)
            //    {
            //        var requestUri = $"{dataStewardshipApiAddress}Caches/{cache}";
            //        success = success && await ClearAsync(client, requestUri);
            //    }
            //}

            //return success;
        }
    }
}
