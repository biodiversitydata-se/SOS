using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Extensions.Dto;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.HealthChecks;

public class HealthJsonResponseTests
{
    [Fact]
    public async Task WriteHealthJsonResponseAsync_PreservesNamesDurationsTagsAndOmitsNullDescriptions()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            ["Database"] = new(HealthStatus.Healthy, "Databasen är tillgänglig", TimeSpan.FromMilliseconds(125), null, null, ["database"]),
            ["WFS"] = new(HealthStatus.Degraded, null, TimeSpan.FromSeconds(1), null, null, ["wfs"])
        }, TimeSpan.FromMilliseconds(1125));
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;

        await HealthCheckExtensions.WriteHealthJsonResponseAsync(context, report);

        Assert.Equal("application/json", context.Response.ContentType);
        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        var result = document.RootElement;
        Assert.Equal(new[] { "duration", "entries", "status" }, result.EnumerateObject().Select(p => p.Name).Order());
        Assert.Equal("Degraded", result.GetProperty("status").GetString());
        Assert.Equal("00:00:01.1250000", result.GetProperty("duration").GetString());
        var entries = result.GetProperty("entries");
        Assert.Equal(2, entries.GetArrayLength());
        var database = entries.EnumerateArray().Single(entry => entry.GetProperty("key").GetString() == "Database");
        Assert.Equal("Databasen är tillgänglig", database.GetProperty("description").GetString());
        Assert.Equal("00:00:00.1250000", database.GetProperty("duration").GetString());
        Assert.Equal("Healthy", database.GetProperty("status").GetString());
        Assert.Equal("database", database.GetProperty("tags")[0].GetString());
        var wfs = entries.EnumerateArray().Single(entry => entry.GetProperty("key").GetString() == "WFS");
        Assert.False(wfs.TryGetProperty("description", out _));
        Assert.Equal("Degraded", wfs.GetProperty("status").GetString());
        Assert.Equal("wfs", wfs.GetProperty("tags")[0].GetString());
    }

    [Fact]
    public async Task WriteHealthJsonResponseAsync_ReturnsEmptyObject_WhenNoReportHasBeenPublished()
    {
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;

        await HealthCheckExtensions.WriteHealthJsonResponseAsync(context, null);

        Assert.Equal("application/json", context.Response.ContentType);
        body.Position = 0;
        using var reader = new StreamReader(body);
        Assert.Equal("{}", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }
}
