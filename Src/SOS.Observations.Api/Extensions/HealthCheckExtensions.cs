using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.HealthChecks;
using SOS.Observations.Api.HealthChecks.Custom;
using System;
using System.Linq;

namespace SOS.Shared.Api.Extensions.Dto;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Create health check result
    /// </summary>
    /// <param name="healthStatus"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static HealthCheckResult ToHealthCheckResult(this HealthStatus healthStatus, string description = null)
    {
        return new HealthCheckResult(healthStatus, description);
    }

    public static IServiceCollection SetupHealthchecks(this IServiceCollection services, bool disableHealthCheckInit, bool isProductionEnvironment)
    {
        var healthChecks = services.AddHealthChecks();
        if (!disableHealthCheckInit)
        {
            services.AddSingleton<IHealthCheckPublisher, HealthReportCachePublisher>();
            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(10);
                options.Period = TimeSpan.FromSeconds(600); // Create new health check every 10 minutes and cache result
                options.Timeout = TimeSpan.FromSeconds(60);
            });
            healthChecks
                //  .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new[] { "database", "mongodb" })
                .AddHangfire(a => a.MinimumAvailableServers = 1, "Hangfire", tags: new[] { "hangfire" })
                .AddCheck<DataAmountHealthCheck>("Data amount", tags: new[] { "database", "elasticsearch", "data" })
                .AddCheck<SearchDataProvidersHealthCheck>("Search data providers", tags: new[] { "database", "elasticsearch", "query" })
                //.AddCheck<SearchPerformanceHealthCheck>("Search performance", tags: new[] { "database", "elasticsearch", "query", "performance" })
                .AddCheck<AzureSearchHealthCheck>("Azure search API health check", tags: new[] { "azure", "database", "elasticsearch", "query" })
                .AddCheck<DataProviderHealthCheck>("Data providers", tags: new[] { "data providers", "meta data" })
                .AddCheck<ElasticsearchProxyHealthCheck>("ElasticSearch Proxy", tags: new[] { "wfs", "elasticsearch" })
                //.AddCheck<DuplicateHealthCheck>("Duplicate observations", tags: new[] { "elasticsearch", "harvest" })
                .AddCheck<ElasticsearchHealthCheck>("Elasticsearch", tags: new[] { "database", "elasticsearch" })
                .AddCheck<DependenciesHealthCheck>("Dependencies", tags: new[] { "dependencies" })                    
                .AddCheck<HealthCheck>("CustomHealthCheck", tags: new[] { "k8s" })
                .AddCheck<IncrementalHarvestHealthCheck>("Incremental harvest", tags: new[] { "harvest" });

            if (isProductionEnvironment)
            {
                healthChecks.AddCheck<APDbRestoreHealthCheck>("Artportalen database backup restore", tags: new[] { "database", "sql server" });
                healthChecks.AddCheck<DwcaHealthCheck>("DwC-A files", tags: new[] { "dwca", "export" });
                healthChecks.AddCheck<ApplicationInsightstHealthCheck>("Application Insights", tags: new[] { "application insights", "harvest" });
                healthChecks.AddCheck<WFSHealthCheck>("WFS", tags: new[] { "wfs" }); // add this to ST environment when we have a GeoServer test environment.
            }
        }

        return services;
    }

    public static WebApplication ApplyMapHealthChecks(this WebApplication app, bool disableHealthCheckInit)
    {
        app.MapHealthChecks("/healthz", new HealthCheckOptions()
        {
            Predicate = r => r.Tags.Contains("k8s")
        });
        if (!disableHealthCheckInit)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => false,
                ResponseWriter = (context, _) => UIResponseWriter.WriteHealthCheckUIResponse(context, HealthReportCachePublisher.LatestNoWfs)
            });
            app.MapHealthChecks("/health-json", new HealthCheckOptions()
            {
                Predicate = _ => false,
                ResponseWriter = async (context, _) =>
                {
                    var report = HealthReportCachePublisher.LatestAll;
                    var result = report == null ? "{}" : Newtonsoft.Json.JsonConvert.SerializeObject(
                        new
                        {
                            status = report.Status.ToString(),
                            duration = report.TotalDuration,
                            entries = report.Entries.Select(e => new
                            {
                                key = e.Key,
                                description = e.Value.Description,
                                duration = e.Value.Duration,
                                status = Enum.GetName(typeof(HealthStatus),
                                    e.Value.Status),
                                tags = e.Value.Tags
                            }).ToList()
                        }, Newtonsoft.Json.Formatting.None,
                        new Newtonsoft.Json.JsonSerializerSettings
                        {
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                        });
                    context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });
            app.MapHealthChecks("/health-wfs", new HealthCheckOptions()
            {
                Predicate = _ => false,
                ResponseWriter = (context, _) => UIResponseWriter.WriteHealthCheckUIResponse(context, HealthReportCachePublisher.LatestOnlyWfs)
            });
        }

        return app;
    }
}