using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Services
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ClamTreeObservationService : Interfaces.IClamTreeObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ClamTreeServiceConfiguration _clamTreeServiceConfiguration;
        private readonly ILogger<ClamTreeObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="clamTreeServiceConfiguration"></param>
        /// <param name="logger"></param>
        public ClamTreeObservationService(
            IHttpClientService httpClientService,
            ClamTreeServiceConfiguration clamTreeServiceConfiguration, 
            ILogger<ClamTreeObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _clamTreeServiceConfiguration = clamTreeServiceConfiguration ?? throw new ArgumentNullException(nameof(clamTreeServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClamObservationVerbatim>> GetClamObservationsAsync()
        {
            return await _httpClientService.GetDataAsync<IEnumerable<ClamObservationVerbatim>>(
                new Uri($"{_clamTreeServiceConfiguration.BaseAddress}/Clams/Observations"));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TreeObservationVerbatim>> GetTreeObservationsAsync(int pageNumber, int pageSize)
        {
            return await _httpClientService.GetDataAsync<IEnumerable<TreeObservationVerbatim>>(
                new Uri($"{_clamTreeServiceConfiguration.BaseAddress}/Trees/Observations/Paged?pageNumber={pageNumber}&pageSize={pageSize}"));
        }
    }
}
