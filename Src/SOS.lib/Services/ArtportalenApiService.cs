using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Helpers;
using SOS.Lib.Models.ArtportalenApiService;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <summary>
    /// Artportalen API service.
    /// </summary>
    public class ArtportalenApiService : IArtportalenApiService
    {
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientService _httpClientService;
        private readonly ArtportalenApiServiceConfiguration _artportalenApiServiceConfiguration;
        private readonly ILogger<ArtportalenApiService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authorizationProvider"></param>
        /// <param name="httpClientService"></param>
        /// <param name="artportalenApiServiceConfiguration"></param>
        /// <param name="logger"></param>
        public ArtportalenApiService(
            IAuthorizationProvider authorizationProvider,
            IHttpClientService httpClientService,
            ArtportalenApiServiceConfiguration artportalenApiServiceConfiguration,
            ILogger<ArtportalenApiService> logger)
        {
            _authorizationProvider =
                authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _artportalenApiServiceConfiguration = artportalenApiServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenApiServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SightingOutput> GetSightingByIdAsync(int sightingId)
        {
            try
            {                
                var sighting = await _httpClientService.GetDataAsync<SightingOutput>(
                    new Uri($"{ _artportalenApiServiceConfiguration.BaseAddress }/sightings/{sightingId}"));

                return sighting;                
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get sighting with sightingId={sightingId} from Artportalen API", e);
            }

            return null;
        }

        public async Task<List<MediaFile>> GetMediaBySightingIdAsync(int sightingId)
        {
            try
            {
                var media = await _httpClientService.GetDataAsync<List<MediaFile>>(
                    new Uri($"{ _artportalenApiServiceConfiguration.BaseAddress }/sightings/{sightingId}/media"));

                return media;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get media for sightingId={sightingId} from Artportalen API", e);
            }

            return null;
        }        
    }
}