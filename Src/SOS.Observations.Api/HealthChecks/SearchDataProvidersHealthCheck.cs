using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    public class SearchDataProvidersHealthCheck : IHealthCheck
    {
        private readonly IObservationManager _observationManager;
        private readonly IDataProviderCache _dataProviderCache;      

        private async Task<(int NumberOfProviders, int SuccessfulProviders, string Msg, HealthStatus Status)> SearchByProviderAsync()
        {
            var providers = (await _dataProviderCache.GetAllAsync())?.
                Where(p => p.IsActive && p.IncludeInHealthCheck);

            if (!providers?.Any() ?? true)
            {
                return (NumberOfProviders: 0, SuccessfulProviders: 0, Msg: "No providers found", Status: HealthStatus.Unhealthy);
            }
            
            var providerSearchTasks = new Dictionary<DataProvider, Task<PagedResult<dynamic>>>();

            foreach (var provider in providers)
            {
                var searchFilter = new SearchFilter(0, ProtectionFilter.Public)
                {
                    DataProviderIds = new List<int>{ provider.Id },
                    Output = new OutputFilter
                    {
                        Fields = new List<string> { "taxon.id" }
                    }
                };
                providerSearchTasks.Add(provider, _observationManager.GetChunkAsync(0, null, searchFilter, 0, 1));
            }

            await Task.WhenAll(providerSearchTasks.Values);

            var providerCount = providers.Count();
            var successfulProviders = providerSearchTasks.Count(t => (t.Value.Result?.TotalCount ?? 0) > 0);
            
            // More than 75% successful. Allow some data providers to contain zero observations.
            if (successfulProviders > providerCount * 0.75)
            {
                return (NumberOfProviders: providerCount, SuccessfulProviders: successfulProviders, Msg: $"{successfulProviders} of {providerCount} data providers returned observations", Status: HealthStatus.Healthy);
            }

            // More than 50% successful
            if (successfulProviders > providerCount * 0.50)
            {
                return (NumberOfProviders: providerCount, SuccessfulProviders: successfulProviders, Msg: $"{successfulProviders} of {providerCount} data providers returned observations", Status: HealthStatus.Degraded);
            }

            return (NumberOfProviders: providerCount, SuccessfulProviders: successfulProviders, Msg: $"{successfulProviders} of {providerCount} data providers returned observations", Status: HealthStatus.Unhealthy);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="dataProviderCache"></param>
        public SearchDataProvidersHealthCheck(IObservationManager observationManager,
            IDataProviderCache dataProviderCache)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
        }

        /// <summary>
        /// Make health check
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await SearchByProviderAsync();
                return new HealthCheckResult(
                    result.Status, 
                    result.Msg,
                    null, 
                    new Dictionary<string, object> {
                        { "SuccessfulProviders", result.SuccessfulProviders },
                        { "NumberOfProviders", result.NumberOfProviders }                        
                    });
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Health check failed", e);
            }
        }
    }
}
