using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileObjects.AgileMapper.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    public class SearchHealthCheck : IHealthCheck
    {
        private readonly IObservationManager _observationManager;
        private readonly IDataProviderCache _dataProviderCache;

        /// <summary>
        /// Search for otters
        /// </summary>
        /// <returns></returns>
        private async Task<HealthStatus> SearchPicaPicaAsync()
        {
            var serachFilter = new SearchFilter()
            {
                TaxonIds = new []{ 103032 },
                IncludeUnderlyingTaxa = true,
                OnlyValidated = false,
                PositiveSightings = false,
                OutputFields = new []
                {
                    "taxon.id"
                }
            };

            var sw = new Stopwatch();
            sw.Start();
            
            var result = await _observationManager.GetChunkAsync(serachFilter, 0, 2, "", SearchSortOrder.Asc);
            sw.Stop();

            return result.TotalCount > 0 ? sw.ElapsedMilliseconds < 500 ? HealthStatus.Healthy : HealthStatus.Degraded : HealthStatus.Unhealthy;
           
        }

        private async Task<HealthStatus> SearchByProviderAsync()
        {
            var providers = (await _dataProviderCache.GetAllAsync())?.
                Where(p => p.IsActive && p.IncludeInHealthCheck);

            if (!providers?.Any() ?? true)
            {
                return HealthStatus.Unhealthy;
            }
            
            var providerSearchTasks = new Dictionary<DataProvider, Task<PagedResult<dynamic>>>();

            foreach (var provider in providers)
            {
                var serachFilter = new SearchFilter()
                {
                    DataProviderIds = new []{ provider.Id },
                    OutputFields = new[]
                    {
                        "taxon.id"
                    }
                };
                providerSearchTasks.Add(provider, _observationManager.GetChunkAsync(serachFilter, 0, 1, "", SearchSortOrder.Asc));
            }

            await Task.WhenAll(providerSearchTasks.Values);

            var providerCount = providers.Count();
            var successfulProviders = providerSearchTasks.Count(t => t.Value.Result.TotalCount > 0);

            // All providers successful
            if (successfulProviders == providerCount)
            {
                return HealthStatus.Healthy;
            }

            // More than 75% successful
            if (successfulProviders > providerCount * 0.75)
            {
                return HealthStatus.Degraded;
            }

            return HealthStatus.Unhealthy;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="dataProviderCache"></param>
        public SearchHealthCheck(IObservationManager observationManager,
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
                var healthTasks = new[]
                {
                    SearchPicaPicaAsync(),
                    SearchByProviderAsync()
                };

                await Task.WhenAll(healthTasks);

                if (healthTasks.All(t => t.Result.Equals(HealthStatus.Healthy)))
                {
                    return new HealthCheckResult(HealthStatus.Healthy, "Everything is alright");
                }

                if (healthTasks.None(t => t.Result.Equals(HealthStatus.Unhealthy)))
                {
                    return new HealthCheckResult(HealthStatus.Degraded, "System working, but not perfect");
                }

                return new HealthCheckResult(HealthStatus.Unhealthy, "System not working as suspected");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Health check failed");
            }
        }
    }
}
