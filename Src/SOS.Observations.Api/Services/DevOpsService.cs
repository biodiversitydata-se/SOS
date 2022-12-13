using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Models.DevOps;

namespace SOS.Observations.Api.Services.Interfaces
{
    /// <summary>
    ///    Dev ops service
    /// </summary>
    public class DevOpsService : IDevOpsService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly DevOpsConfiguration _devOpsConfiguration;
        private readonly ILogger<DevOpsService> _logger;
        private readonly IDictionary<string, string> _headerData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="devOpsConfiguration"></param>
        /// <param name="logger"></param>
        public DevOpsService(
            IHttpClientService httpClientService,
            DevOpsConfiguration devOpsConfiguration,
            ILogger<DevOpsService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _devOpsConfiguration = devOpsConfiguration ?? throw new ArgumentNullException(nameof(devOpsConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _headerData = new Dictionary<string, string>
            {
                { "personalAccessToken", _devOpsConfiguration.PersonalAccessToken }
            };
        }
        /// <summary>
        /// Get releases
        /// </summary>
        /// <param name="definitionId"></param>
        /// <returns></returns>
        public async Task<MultiResponse<ReleaseDefinition>> GetReleasesAsync(int definitionId)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<MultiResponse<ReleaseDefinition>>(
                    new Uri($"{_devOpsConfiguration.BaseAddress}/release/releases?definitionId={definitionId}"), _headerData);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get releases from dev ops", e);
            }

            return null;
        }

        /// <summary>
        /// Get a release by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Release> GetReleaseAsync(int id)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<Release>(
                     new Uri($"{_devOpsConfiguration.BaseAddress}/release/releases/{id}"), _headerData);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get release from dev ops", e);
            }

            return null;
        }
    }
}