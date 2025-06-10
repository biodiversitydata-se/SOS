using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers;
public static class SeriLogHelper
{
    public static Logger CreateLogger(bool isLocalDevelopment)
    {
        var logger = isLocalDevelopment ?
            new LoggerConfiguration() // human readable in the terminal when developing, not all json
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties}{NewLine}{Exception}")
            .CreateLogger()
        :
            new LoggerConfiguration() // compact json when running in the clusters for that sweet sweet structured logging
                .MinimumLevel.Information()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
            .CreateLogger();

        return logger;
    }

    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(builder.Environment.EnvironmentName?.ToLower(), StringComparer.CurrentCultureIgnoreCase);

        if (isLocalDevelopment)
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties}{NewLine}{Exception}")
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
                    foreach (var header in headers)
                    {
                        var (key, value) = header.Split('=') switch
                        {
                            [string k, string v] => (k, v),
                            var v => throw new Exception($"Invalid header format {v}")
                        };

                        options.Headers.Add(key, value);
                    }
                    options.ResourceAttributes.Add("service.name", "apiservice");

                    //To remove the duplicate issue, we can use the below code to get the key and value from the configuration
                    string otelResourceAttributes = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"];
                    if (!string.IsNullOrEmpty(otelResourceAttributes))
                    {
                        var (otelResourceAttribute, otelResourceAttributeValue) = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]?.Split('=') switch
                        {
                            [string k, string v] => (k, v),
                            _ => throw new Exception($"Invalid header format {builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]}")
                        };

                        options.ResourceAttributes.Add(otelResourceAttribute, otelResourceAttributeValue);
                    }
                })
                .ReadFrom.Configuration(ctx.Configuration)
            );
        }
        else
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
                    foreach (var header in headers)
                    {
                        var (key, value) = header.Split('=') switch
                        {
                            [string k, string v] => (k, v),
                            var v => throw new Exception($"Invalid header format {v}")
                        };

                        options.Headers.Add(key, value);
                    }
                    options.ResourceAttributes.Add("service.name", "apiservice");

                    //To remove the duplicate issue, we can use the below code to get the key and value from the configuration
                    string otelResourceAttributes = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"];
                    if (!string.IsNullOrEmpty(otelResourceAttributes))
                    {
                        var (otelResourceAttribute, otelResourceAttributeValue) = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]?.Split('=') switch
                        {
                            [string k, string v] => (k, v),
                            _ => throw new Exception($"Invalid header format {builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]}")
                        };

                        options.ResourceAttributes.Add(otelResourceAttribute, otelResourceAttributeValue);
                    }
                })
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
                .ReadFrom.Configuration(ctx.Configuration)
            );
        }
    }
}
