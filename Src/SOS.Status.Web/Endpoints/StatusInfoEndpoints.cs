using SOS.Status.Web.Services;

namespace SOS.Status.Web.Endpoints;

public static class StatusInfoEndpoints
{
    public static IEndpointRouteBuilder MapStatusInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/status-info/process-summary",
            async (StatusInfoService manager) =>
                await manager.GetProcessSummaryAsync());

        app.MapGet("/api/status-info/process-info",
            async (StatusInfoService manager) =>
                await manager.GetProcessInfoAsync());

        app.MapGet("/api/status-info/process-info/{active:bool}",
            async (StatusInfoService manager, bool active) =>
                await manager.GetProcessInfoAsync(active));

        app.MapGet("/api/status-info/data-provider-status",
            async (StatusInfoService manager) =>
                await manager.GetDataProviderStatusAsync());

        app.MapGet("/api/status-info/data-provider-status-rows",
            async (StatusInfoService manager) =>
                await manager.GetDataProviderStatusRowsAsync());

        app.MapGet("/api/status-info/health/observations",
            async (StatusInfoService manager) =>
                await manager.GetObservationsApiHealthAsync());

        app.MapGet("/api/status-info/health/analysis",
            async (StatusInfoService manager) =>
                await manager.GetAnalysisApiHealthAsync());

        app.MapGet("/api/status-info/health/elasticsearch-proxy",
            async (StatusInfoService manager) =>
                await manager.GetElasticsearchProxyHealthAsync());

        app.MapGet("/api/status-info/health/data-stewardship",
            async (StatusInfoService manager) =>
                await manager.GetDataStewardshipApiHealthAsync());

        return app;
    }
}
