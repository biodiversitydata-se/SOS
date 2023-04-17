using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;

namespace SOS.DataStewardship.Api.HealthChecks
{
    public class EventHealthCheck : IHealthCheck
    {
        private readonly IDataStewardshipManager _dataStewardshipManager;

        private async Task<HealthStatus> TestEventsAsync()
        {
            var events = await _dataStewardshipManager.GetEventsBySearchAsync(new Contracts.Models.EventsFilter
            {
                DateFilter = new Contracts.Models.DateFilter
                {
                    DateFilterType = Contracts.Enums.DateFilterType.OnlyStartDate,
                    StartDate = new DateTime(2000, 1, 1)
                }
            }, 0, 1, Contracts.Enums.CoordinateSystem.EPSG4326);
            
            if (events?.Records?.Any() ?? 0 > 0)
            {
                var eventId = events.Records.First().EventID;

                var @event = await _dataStewardshipManager.GetEventByIdAsync(eventId, Contracts.Enums.CoordinateSystem.EPSG4326);

                if (@event?.EventID.Equals(eventId, StringComparison.CurrentCultureIgnoreCase) ?? false)
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
        public EventHealthCheck(IDataStewardshipManager dataStewardshipManager)
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
                var result = await TestEventsAsync();
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
