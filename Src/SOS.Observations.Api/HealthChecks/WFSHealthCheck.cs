using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks;

/// <summary>
/// Health check by checking number of documents in index 
/// </summary>
public class WFSHealthCheck : IHealthCheck
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<WFSHealthCheck> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClientService"></param>
    /// <param name="logger"></param>
    public WFSHealthCheck(IHttpClientService httpClientService, ILogger<WFSHealthCheck> logger)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            System.Text.Json.JsonElement response = await GetResponseAsync(new Uri("https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10"), cancellationToken);
            if (response.GetProperty("numberReturned").GetInt16() != 10)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get observations");
            }

            response = await GetResponseAsync(new Uri("https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10&srsName=EPSG:3006"), cancellationToken);
            if (response.GetProperty("numberReturned").GetInt16() != 10)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get observations in SWEREF99 TM coordinate system");
            }

            response = await GetResponseAsync(new Uri("https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10&CQL_Filter=organismGroup='Kärlväxter'"), cancellationToken);
            if (response.GetProperty("numberReturned").GetInt16() != 10)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get observations of a specific organism group by using CQL filter");
            }

            response = await GetResponseAsync(new Uri("https://sosgeo.artdata.slu.se/geoserver/SOS/ows?SERVICE=WFS&Request=GetFeature&Version=2.0.0&Typenames=SOS:SpeciesObservations&StartIndex=20&count=10&outputFormat=application/json"), cancellationToken);
            if (response.GetProperty("numberReturned").GetInt16() != 10)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get observations by using paging");
            }

            return new HealthCheckResult(HealthStatus.Healthy, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, "WFS health check timed out");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "WFS Health check error");
            return new HealthCheckResult(HealthStatus.Unhealthy, $"WFS health check failed. {e.Message}, {e.StackTrace}");
        }
    }

    private async Task<System.Text.Json.JsonElement> GetResponseAsync(Uri uri, CancellationToken cancellationToken)
    {
        return await _httpClientService
            .GetDataAsync<System.Text.Json.JsonElement>(uri, null, RequestTimeout)
            .WaitAsync(cancellationToken);
    }
}
