using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Services
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
    }
}