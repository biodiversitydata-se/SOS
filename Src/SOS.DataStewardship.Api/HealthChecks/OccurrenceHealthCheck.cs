using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;

namespace SOS.DataStewardship.Api.HealthChecks
{
    public class OccurrenceHealthCheck : IHealthCheck
    {
        private readonly IDataStewardshipManager _dataStewardshipManager;

        private async Task<HealthStatus> TestOccurrencesAsync()
        {
            var occurrences = await _dataStewardshipManager.GetOccurrencesBySearchAsync(new Contracts.Models.OccurrenceFilter
            {
                DateFilter = new Contracts.Models.DateFilter
                {
                    DateFilterType = Contracts.Enums.DateFilterType.OnlyStartDate,
                    StartDate = new DateTime(2000, 1, 1)
                }
            }, 0, 1, Contracts.Enums.CoordinateSystem.EPSG4326);

            if (occurrences?.Records?.Any() ?? 0 > 0)
            {
                var occurrenceId = occurrences.Records.First().OccurrenceID;

                var occurrence = await _dataStewardshipManager.GetOccurrenceByIdAsync(occurrenceId, Contracts.Enums.CoordinateSystem.EPSG4326);

                if (occurrence?.OccurrenceID?.Equals(occurrenceId, StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    return HealthStatus.Healthy;
                }
                return HealthStatus.Degraded;
            }

            return HealthStatus.Unhealthy;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataStewardshipManager"></param>
        public OccurrenceHealthCheck(IDataStewardshipManager dataStewardshipManager)
        {
            _dataStewardshipManager = dataStewardshipManager ?? throw new ArgumentNullException(nameof(dataStewardshipManager));
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
                var result = await TestOccurrencesAsync();
                return new HealthCheckResult(
                    result);
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Health check failed", e);
            }
        }
    }
}
