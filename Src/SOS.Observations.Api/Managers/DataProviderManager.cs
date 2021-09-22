using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Extensions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Data provider manager.
    /// </summary>
    public class DataProviderManager : IDataProviderManager
    {
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ILogger<DataProviderManager> _logger;
        private readonly IProcessInfoManager _processInfoManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderCache"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="logger"></param>
        public DataProviderManager(
            IDataProviderCache dataProviderCache,
            IProcessInfoManager processInfoManager,
            ILogger<DataProviderManager> logger)
        {
            _dataProviderCache =
                dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode, bool includeProvidersWithNoObservations = true)
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
                    if (!includeProvidersWithNoObservations &&
                        providerInfo.PublicProcessCount.GetValueOrDefault(0) == 0 &&
                        providerInfo.ProtectedProcessCount.GetValueOrDefault(0) == 0)
                    {
                        continue;
                    }

                    dataProviderDtos.Add(DataProviderDto.Create(
                        dataProvider,
                        providerInfo.PublicProcessCount.GetValueOrDefault(0),
                        providerInfo.ProtectedProcessCount.GetValueOrDefault(0),
                        providerInfo.HarvestEnd,
                        providerInfo.HarvestNotes,
                        providerInfo.ProcessEnd,
                        providerInfo.LatestIncrementalEnd,
                        cultureCode));
                }
                else
                {
                    if (includeProvidersWithNoObservations)
                    {
                        dataProviderDtos.Add(DataProviderDto.Create(dataProvider, cultureCode));
                    }
                }
            }

            return dataProviderDtos;
        }

        public async Task<byte[]> GetEmlFileAsync(int providerId)
        {
            var eml = await _dataProviderCache.GetEmlAsync(providerId);

            return await eml?.ToBytesAsync();
        }
    }
}