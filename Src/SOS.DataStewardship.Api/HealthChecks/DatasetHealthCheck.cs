using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;

namespace SOS.DataStewardship.Api.HealthChecks
{
    public class DatasetHealthCheck : IHealthCheck
    {
        private readonly IDataStewardshipManager _dataStewardshipManager;

        private async Task<HealthStatus> TestDatasetsAsync()
        {
            var datasets = await _dataStewardshipManager.GetDatasetsBySearchAsync(new Contracts.Models.DatasetFilter
            {
                DateFilter = new Contracts.Models.DateFilter
                {
                    DateFilterType = Contracts.Enums.DateFilterType.OnlyStartDate,
                    StartDate = new DateTime(2000, 1, 1)
                }
            }, 0, 1);

            if (datasets?.Records?.Any() ?? 0 > 0)
            {
                var datasetId = datasets.Records.First().Identifier;
                var dataset = await _dataStewardshipManager.GetDatasetByIdAsync(datasetId);

                if (dataset?.Identifier.Equals(datasetId, StringComparison.CurrentCultureIgnoreCase) ?? false)
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
        public DatasetHealthCheck(IDataStewardshipManager dataStewardshipManager)
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
                var result = await TestDatasetsAsync();
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
