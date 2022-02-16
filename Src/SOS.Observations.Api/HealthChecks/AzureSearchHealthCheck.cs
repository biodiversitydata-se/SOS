using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Observations.Api.Dtos.Filter;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check for SOS using Azure API.
    /// </summary>
    public partial class AzureSearchHealthCheck : IHealthCheck
    {
        private readonly HealthCheckConfiguration _healthCheckConfiguration;         
        private readonly SosAzureClient _sosAzureClient;
        private bool _intialized; // true if Azure API URL and subscription key is set.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="healthCheckConfiguration"></param>
        public AzureSearchHealthCheck(HealthCheckConfiguration healthCheckConfiguration)
        {            
            _healthCheckConfiguration = healthCheckConfiguration ?? throw new ArgumentNullException(nameof(healthCheckConfiguration));
            if (healthCheckConfiguration != null && !string.IsNullOrEmpty(healthCheckConfiguration.AzureApiUrl) && !string.IsNullOrEmpty(healthCheckConfiguration.AzureSubscriptionKey))
            {
                _sosAzureClient = new SosAzureClient(healthCheckConfiguration.AzureApiUrl, healthCheckConfiguration.AzureSubscriptionKey);
                _intialized = true;
            }            
        }
       
        private async Task<(TimeSpan Duration, long TotalCount)> SearchPicaPicaAsync()
        {
            var searchFilter = new SearchFilterDto()
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 103032 },
                    IncludeUnderlyingTaxa = true
                },                
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
                Output = new OutputFilterDto
                {
                    Fields = new[] { "taxon.id" }
                }                
            };

            // Warm up
            await _sosAzureClient.SearchObservations(searchFilter, "sv-SE", 0, 2);            

            Thread.Sleep(1100);
            var sw = new Stopwatch();
            sw.Start();
            var result = await _sosAzureClient.SearchObservations(searchFilter, "sv-SE", 0, 2);
            sw.Stop();

            return (Duration: sw.Elapsed, TotalCount: result.TotalCount);
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
                if (!_intialized)
                {
                    return new HealthCheckResult(HealthStatus.Healthy, $"Azure API URL and/or Subscription key not set.");
                }

                var dataproviders = await _sosAzureClient.GetDataProviders("sv-SE");
                if (dataproviders == null || dataproviders.Count <= 10)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"Couldn't retrieve data providers using Azure API.");
                }

                Thread.Sleep(1100);

                var searchResult = await SearchPicaPicaAsync();
                if (searchResult.TotalCount == 0)
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, $"No observations returned. Duration: {searchResult.Duration.TotalMilliseconds:N0}ms");
                }

                if (searchResult.Duration.TotalMilliseconds > 5000)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"Duration is > 5000ms. Duration: {searchResult.Duration.TotalMilliseconds:N0}ms");
                }

                return new HealthCheckResult(HealthStatus.Healthy, $"Duration is {searchResult.Duration.TotalMilliseconds:N0}ms");

            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Degraded, "Health check failed", e);
            }
        }
    }
}