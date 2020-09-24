using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class DataCiteService : IDataCiteService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly DataCiteServiceConfiguration _dataCiteServiceConfiguration;
        private readonly ILogger<DataCiteService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="dataCiteServiceConfiguration"></param>
        /// <param name="logger"></param>
        public DataCiteService(
            IHttpClientService httpClientService,
            DataCiteServiceConfiguration dataCiteServiceConfiguration,
            ILogger<DataCiteService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _dataCiteServiceConfiguration = dataCiteServiceConfiguration ??
                                            throw new ArgumentNullException(nameof(dataCiteServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> CreateDoiAsync()
        {
            return await _httpClientService.GetDataAsync<bool>(
                new Uri($"{_dataCiteServiceConfiguration.BaseAddress}/Todo"));
        }

        /// <inheritdoc />
        public async Task<Models.DataCite.DOIResponse<Models.DataCite.DOIMetadata>> GetMetadataAsync(string prefix, string suffix)
        {
            try
            {
                return await _httpClientService.GetDataAsync<Models.DataCite.DOIResponse<Models.DataCite.DOIMetadata>>(
                    new Uri($"{ _dataCiteServiceConfiguration.BaseAddress }/dois/{ prefix }/{ suffix }"));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user roles", e);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<Models.DataCite.DOIResponse<IEnumerable<Models.DataCite.DOIMetadata>>> SearchMetadataAsync(string searchFor)
        {
            try
            {
                return await _httpClientService.GetDataAsync<Models.DataCite.DOIResponse<IEnumerable<Models.DataCite.DOIMetadata>>>(
                    new Uri($"{ _dataCiteServiceConfiguration.BaseAddress }/dois?client-id=gbif.gbif&query=+{searchFor.Replace(" ", "+")}&sort=created:desc"));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to search DOI's", e);
            }

            return null;
        }
    }
}