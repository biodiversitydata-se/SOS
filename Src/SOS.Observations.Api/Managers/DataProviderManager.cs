using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Data provider manager.
    /// </summary>
    public class DataProviderManager : IDataProviderManager
    {
        private readonly ICache<int, DataProvider> _dataProviderCache;
        private readonly ILogger<DataProviderManager> _logger;
        private readonly IProcessInfoManager _processInfoManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderCache"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="logger"></param>
        public DataProviderManager(
            ICache<int, DataProvider> dataProviderCache,
            IProcessInfoManager processInfoManager,
            ILogger<DataProviderManager> logger)
        {
            _dataProviderCache =
                dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode)
        {
            var dataProviderDtos = new List<DataProviderDto>();
            var processInfosActive = await _processInfoManager.GetProcessInfoAsync(true);
            var allDataProviders = await _dataProviderCache.GetAllAsync();
            var selectedDataProviders = includeInactive
                ? allDataProviders
                : allDataProviders.Where(provider => provider.IsActive).ToList();

            // Add process data
            foreach (var dataProvider in selectedDataProviders)
            {
               
                var providerInfo =
                    processInfosActive?.ProvidersInfo?.FirstOrDefault(provider => provider.DataProviderId == dataProvider.Id);

                if (providerInfo != null)
                {
                    dataProviderDtos.Add(DataProviderDto.Create(
                        dataProvider,
                        providerInfo.ProcessCount.GetValueOrDefault(0),
                        0,
                        providerInfo.HarvestEnd,
                        providerInfo.ProcessEnd,
                        providerInfo.LatestIncrementalEnd,
                        cultureCode));
                }
                else
                {
                    dataProviderDtos.Add(DataProviderDto.Create(dataProvider, cultureCode));
                }
            }

            return dataProviderDtos;
        }
    }
}