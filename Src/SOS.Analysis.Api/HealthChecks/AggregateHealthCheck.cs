using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.HealthChecks
{
    public class AggregateHealthCheck : IHealthCheck
    {
        private readonly IAnalysisManager _analysisManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="analysisManager"></param>
        public AggregateHealthCheck(IAnalysisManager analysisManager)
        {
            _analysisManager = analysisManager ?? throw new ArgumentNullException(nameof(analysisManager));
        }

        private async Task<bool> AggregateAsync()
        {

            var filter = new SearchFilter(0)
            {
                Taxa = new TaxonFilter
                {
                    Ids = new[] { 103032 },
                    IncludeUnderlyingTaxa = true
                },
                VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                PositiveSightings = true
            };

            var result = await _analysisManager.AggregateByUserFieldAsync(
                null,
                null,
                filter,
                "dataProviderId",
                false,
                1,
                1,
                Lib.Models.Search.Enums.AggregationSortOrder.KeyAscending
            );

            return result.Count() != 0;
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
                var success = await AggregateAsync();
                if (!success)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"Aggregation didn't succeed");
                }
                return new HealthCheckResult(HealthStatus.Healthy, $"Aggregation succeed");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Aggregation failed", e);
            }
        }
    }
}