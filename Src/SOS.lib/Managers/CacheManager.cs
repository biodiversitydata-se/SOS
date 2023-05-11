using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Lib.Managers
{
    /// <inheritdoc />
    public class CacheManager : ICacheManager
    {
        private readonly SosApiConfiguration _sosApiConfiguration;
        private readonly ILogger<CacheManager> _logger;

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

            return success;
        }
    }
}
