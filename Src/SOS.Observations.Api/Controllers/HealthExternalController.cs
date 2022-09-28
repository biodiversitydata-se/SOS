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
using SOS.Observations.Api.Dtos.Health;
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
        [ProducesResponseType(typeof(IEnumerable<HealthEntryDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var healthCheck = await _healthCheckService.CheckHealthAsync(new System.Threading.CancellationToken());

                var checks = new List<HealthEntryDto>();
                var azureApiCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Azure search API health check"))?.Select(d => d.Value)?.FirstOrDefault();
                checks.Add(new HealthEntryDto
                {
                    Name = "SOS API",
                    Description = azureApiCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Not running" : "Running",
                    Status = azureApiCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Unhealthy" : "Healthy"
                });
                var dataprovidersCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Search data providers"))?.Select(d => d.Value)?.FirstOrDefault();
                checks.Add(new HealthEntryDto
                {
                    Name = "Data providers",
                    Description = $"{dataprovidersCheck.Value.Data.Where(d => d.Key.Equals("SuccessfulProviders")).Select(d => d.Value).FirstOrDefault()} data providers",
                    Status = dataprovidersCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Unhealthy" : "Healthy"
                });
                var dataAmountCheck = healthCheck.Entries?.Where(e => e.Key.Equals("Data amount"))?.Select(d => d.Value)?.FirstOrDefault();
                checks.Add(new HealthEntryDto
                {
                    Name = "Observations",
                    Description = $"{dataAmountCheck.Value.Data.Where(d => d.Key.Equals("Public observations")).Select(d => d.Value).FirstOrDefault()} public observations, {dataAmountCheck.Value.Data.Where(d => d.Key.Equals("Protected observations")).Select(d => d.Value).FirstOrDefault()} protected observations",
                    Status = dataAmountCheck.Value.Status.Equals(HealthStatus.Unhealthy) ? "Unhealthy" : "Healthy"
                });
                var wfsCheckStatus = healthCheck.Entries?.Where(e => e.Key.Equals("WFS"))?.Select(e => e.Value)?.FirstOrDefault().Status;
                checks.Add(new HealthEntryDto
                {
                    Name = "WFS",
                    Description = wfsCheckStatus.Equals(HealthStatus.Unhealthy) ? "Not running" : "Running",
                    Status = wfsCheckStatus.Equals(HealthStatus.Unhealthy) ? "Unhealthy" : "Healthy"
                });
                var processInfo = await _processInfoManager.GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);
                checks.Add(new HealthEntryDto
                {
                    Name = "Latest Full harvest",
                    Description = processInfo?.End.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss"),
                    Status = (processInfo?.End ?? DateTime.MinValue).ToLocalTime() > DateTime.Now.AddDays(-1) ? "Healthy" : "Unhealthy"
                });
                var apProvider = processInfo?.ProvidersInfo.Where(p => p.DataProviderId.Equals(1)).FirstOrDefault();
                checks.Add(new HealthEntryDto
                {
                    Name = "Latest Artportalen incremental harvest",
                    Description = apProvider?.LatestIncrementalEnd.HasValue ?? false ? apProvider.LatestIncrementalEnd.Value.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss") : "",
                    Status = (apProvider?.LatestIncrementalEnd.HasValue ?? false) && apProvider?.LatestIncrementalEnd.Value.ToLocalTime() > DateTime.Now.AddMinutes(-10)  ? "Healthy" : "Unhealthy"
                });

                return new OkObjectResult(checks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error making health check");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}