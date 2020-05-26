using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Data provider manager.
    /// </summary>
    public class DataProviderManager : IDataProviderManager
    {
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly IProcessInfoManager _processInfoManager;
        private readonly ILogger<DataProviderManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderRepository"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="logger"></param>
        public DataProviderManager(
            IDataProviderRepository dataProviderRepository,
            IProcessInfoManager processInfoManager,
            ILogger<DataProviderManager> logger)
        {
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DataProvider>> GetDataProvidersAsync()
        {
            var processInfo = await _processInfoManager.GetProcessInfoAsync(true);
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            
            // Add process data
            foreach (var dataProvider in allDataProviders)
            {
                var providerInfo = processInfo?.ProvidersInfo?.FirstOrDefault(provider => provider.DataProviderId == dataProvider.Id);
                if (providerInfo != null)
                {
                    dataProvider.LatestHarvestDate = providerInfo.HarvestEnd;
                    dataProvider.PublicObservations = providerInfo.ProcessCount.GetValueOrDefault(0);
                }
            }

            return allDataProviders;
        }
    }
}