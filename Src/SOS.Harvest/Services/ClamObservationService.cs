using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Services
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ClamObservationService : IClamObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ClamServiceConfiguration _clamServiceConfiguration;
        private readonly ILogger<ClamObservationService> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="clamServiceConfiguration"></param>
        /// <param name="logger"></param>
        public ClamObservationService(
            IHttpClientService httpClientService,
            ClamServiceConfiguration clamServiceConfiguration,
            ILogger<ClamObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _clamServiceConfiguration = clamServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(clamServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClamObservationVerbatim>> GetClamObservationsAsync()
        {
            return await _httpClientService.GetDataAsync<IEnumerable<ClamObservationVerbatim>>(
                new Uri($"{_clamServiceConfiguration.BaseAddress}/Clams/Observations"));
        }
    }
}