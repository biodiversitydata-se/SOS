using System;
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
            try
            {
                var client = new HttpClient();
                foreach (var observationsApiAddress in _sosApiConfiguration.ObservationsApiAddresses)
                {
                    var requestUri = $"{observationsApiAddress}Caches/{cache}";
                    var response = await client.DeleteAsync(requestUri);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Failed to clear {cache} cache on ({observationsApiAddress})");
                        return false;
                    }

                    _logger.LogInformation($"{cache} cache cleared ({observationsApiAddress})");
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to clear {cache} cache");
                return false;
            }
        }
    }
}
