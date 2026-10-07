using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Extensions;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers;

/// <summary>
///     Data provider manager.
/// </summary>
public class DataProviderManager : IDataProviderManager
{
    private readonly IDataProviderCache _dataProviderCache;
    private readonly IProcessInfoManager _processInfoManager;
    private readonly IProcessedObservationRepository _processedObservationRepository;
    private readonly ILogger<DataProviderManager> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataProviderCache"></param>
    /// <param name="processInfoManager"></param>
    /// <param name="processedObservationRepository"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DataProviderManager(
        IDataProviderCache dataProviderCache,
        IProcessInfoManager processInfoManager,
        IProcessedObservationRepository processedObservationRepository,
        ILogger<DataProviderManager> logger)
    {
        _dataProviderCache =
            dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
        _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
        _processedObservationRepository = processedObservationRepository ??
                                          throw new ArgumentNullException(nameof(processedObservationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode, bool includeProvidersWithNoObservations = true, IEnumerable<SOS.Lib.Enums.DataProviderCategory> categories = null)
    {
        var dataProviderDtos = new List<DataProviderDto>();
        var processInfosActive = await _processInfoManager.GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);
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
                if ((categories?.Any() ?? false) && !(dataProvider.Categories?.Any(c => categories.Contains(c)) ?? true))
                {
                    continue;
                }

                var dto = CreateDataProviderDto(dataProvider, cultureCode);
                dto.PublicObservations = providerInfo.PublicProcessCount.GetValueOrDefault(0);
                dto.ProtectedObservations = providerInfo.ProtectedProcessCount.GetValueOrDefault(0);
                dto.LatestHarvestDate = providerInfo.HarvestEnd;
                dto.HarvestNotes = providerInfo.HarvestNotes;
                dto.LatestProcessDate = providerInfo.ProcessEnd;
                dto.LatestIncrementalHarvestDate = providerInfo.LatestIncrementalEnd;
                dto.NextHarvestFrom = dataProvider.NextHarvestFrom(providerInfo.HarvestEnd?.UtcDateTime).ToUtcDateTimeOffset();
                dataProviderDtos.Add(dto);
            }
            else
            {
                dataProviderDtos.Add(CreateDataProviderDto(dataProvider, cultureCode));
            }
        }

        return dataProviderDtos;
    }

    private static DataProviderDto CreateDataProviderDto(DataProvider dataProvider, string cultureCode)
    {
        return new DataProviderDto
        {
            Id = dataProvider.Id,
            Identifier = dataProvider.Identifier,
            Name = dataProvider.Names?.Translate(cultureCode),
            Description = dataProvider.Descriptions?.Translate(cultureCode),
            Organization = dataProvider.Organizations?.Translate(cultureCode),
            Path = dataProvider.Paths?
                .Where(path => path.CultureCode?.Equals(cultureCode, StringComparison.CurrentCultureIgnoreCase) ?? false)
                .SelectMany(path => path.Path)
                .ToList(),
            Url = dataProvider.Url,
            NextHarvestFrom = dataProvider.NextHarvestFrom(null).ToUtcDateTimeOffset(),
            HarvestSchedule = dataProvider.HarvestSchedule,
            IncludeInSearchByDefault = dataProvider.IncludeInSearchByDefault
        };
    }

    public async Task<byte[]> GetEmlFileAsync(int providerId)
    {
        var eml = await _dataProviderCache.GetEmlAsync(providerId);
        if (eml == null)
        {
            return Array.Empty<byte>();
        }

        return await eml?.ToBytesAsync();
    }
}
