using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Sighting controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class HealthExternalController : ControllerBase, IHealthExternalController
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly IProcessInfoManager _processInfoManager;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<HealthExternalController> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="healthCheckService"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HealthExternalController(HealthCheckService healthCheckService,
            IProcessInfoManager processInfoManager,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<HealthExternalController> logger)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet()]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var start = DateTime.Now;
                var healthCheck = await _healthCheckService.CheckHealthAsync(new System.Threading.CancellationToken());
                var entries = new Dictionary<string, HealthReportEntry>();
              
                var azureApiCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Azure search API health check"))?.Select(d => d.Value)?.FirstOrDefault();
                var azureStatus = azureApiCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Not running" : "Running";
                entries.Add("SOS API", new HealthReportEntry(
                    azureApiCheck.Value.Status,
                    azureStatus,
                    azureApiCheck.Value.Duration,
                    null,
                    new Dictionary<string, object>()
                       {
                           { "Status", azureStatus }
                       }
                    )
                );
                var dataprovidersCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Search data providers"))?.Select(d => d.Value)?.FirstOrDefault();
                var successfullProviders = dataprovidersCheck.Value.Data.Where(d => d.Key.Equals("SuccessfulProviders")).Select(d => d.Value).FirstOrDefault();
                entries.Add("Data providers", new HealthReportEntry(
                    dataprovidersCheck.Value.Status,
                    $"{successfullProviders} data providers",
                    dataprovidersCheck.Value.Duration,
                    null,
                    new Dictionary<string, object>()
                       {
                           { "ProviderCount", successfullProviders }
                       }
                    )
                );
                var dataAmountCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Data amount"))?.Select(d => d.Value)?.FirstOrDefault();
                var publicCount = dataAmountCheck.Value.Data.Where(d => d.Key.Equals("Public observations")).Select(d => d.Value).FirstOrDefault();
                var protectedCount = dataAmountCheck.Value.Data.Where(d => d.Key.Equals("Protected observations")).Select(d => d.Value).FirstOrDefault();
                entries.Add("Observations", new HealthReportEntry(
                    dataAmountCheck.Value.Status,
                    $"{publicCount} public observations, {protectedCount} protected observations",
                    dataAmountCheck.Value.Duration,
                    null,
                    new Dictionary<string, object>()
                       {
                           { "PublicCount", publicCount },
                           { "ProtectedCount", protectedCount }
                       }
                    )
                );
                var wfsCheck = healthCheck.Entries?.Where(e => e.Key.Equals("WFS"))?.Select(e => e.Value)?.FirstOrDefault();
                var wfsStatus = wfsCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Not running" : "Running";
                entries.Add("WFS", new HealthReportEntry(
                    wfsCheck.Value.Status,
                    wfsStatus,
                    wfsCheck.Value.Duration,
                    null,
                    new Dictionary<string, object>()
                       {
                           { "Status", wfsStatus }
                       }
                    )
                );
                var processInfo = await _processInfoManager.GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);
                var processingEnd = processInfo?.End.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss");
                entries.Add("Latest Full harvest", new HealthReportEntry(
                    (processInfo?.End ?? DateTime.MinValue).ToLocalTime() > DateTime.Now.AddDays(-1) ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                    processingEnd,
                    TimeSpan.FromTicks(0),
                    null,
                    new Dictionary<string, object>()
                       {
                           { "ProcessingEnd", processingEnd }
                       }
                    )
                );
                var apProvider = processInfo?.ProvidersInfo.Where(p => p.DataProviderId.Equals(1)).FirstOrDefault();
                var apIncrementalEnd = apProvider?.LatestIncrementalEnd.HasValue ?? false ? apProvider?.LatestIncrementalEnd.Value.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss") : "";
                entries.Add("Latest Artportalen incremental harvest", new HealthReportEntry(
                    (apProvider?.LatestIncrementalEnd.HasValue ?? false) && apProvider?.LatestIncrementalEnd.Value.ToLocalTime() > DateTime.Now.AddMinutes(-10) ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                    apIncrementalEnd,
                    TimeSpan.FromTicks(0),
                    null,
                    new Dictionary<string, object>()
                       {
                           { "IncrementalEnd", apIncrementalEnd }
                       }
                    )
                );

                return new OkObjectResult(new HealthReport(entries, DateTime.Now - start));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error making health check");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}