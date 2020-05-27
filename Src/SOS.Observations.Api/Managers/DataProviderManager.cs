using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Observations.Api.Dtos;
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
        public async Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive)
        {
            List<DataProviderDto> dataProviderDtos = new List<DataProviderDto>();
            var processInfo = await _processInfoManager.GetProcessInfoAsync(true);
            List<DataProvider> allDataProviders = await _dataProviderRepository.GetAllAsync();
            var selectedDataProviders = includeInactive ? allDataProviders : allDataProviders.Where(provider => provider.IsActive).ToList();

            // Add process data
            foreach (var dataProvider in selectedDataProviders)
            {
                var providerInfo = processInfo?.ProvidersInfo?.FirstOrDefault(provider => provider.DataProviderId == dataProvider.Id);
                if (providerInfo != null)
                {
                    dataProviderDtos.Add(DataProviderDto.Create(
                        dataProvider,
                        providerInfo.ProcessCount.GetValueOrDefault(0),
                        0,
                        providerInfo.HarvestEnd));
                }
                else
                {
                    dataProviderDtos.Add(DataProviderDto.Create(dataProvider));
                }
            }

            return dataProviderDtos;
        }
    }
}