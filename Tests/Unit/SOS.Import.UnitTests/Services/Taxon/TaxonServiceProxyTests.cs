using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Taxon;
using Xunit;

namespace SOS.Import.UnitTests.Services.Taxon;

public class TaxonServiceProxyTests
{
    [Fact]
    public async Task GetTaxonAsync_PreservesRequestPropertyNameAndTaxonIds()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        string? requestBody = null;
        string? contentType = null;
        app.MapPost("/taxa", async context =>
        {
            using var reader = new StreamReader(context.Request.Body);
            requestBody = await reader.ReadToEndAsync(context.RequestAborted);
            contentType = context.Request.ContentType;
            await context.Response.WriteAsync("{\"taxa\":[]}", context.RequestAborted);
        });
        await app.StartAsync(TestContext.Current.CancellationToken);
        var address = app.Services.GetRequiredService<IServer>().Features
            .Get<IServerAddressesFeature>()!.Addresses.Single();

        var result = await new TaxonServiceProxy().GetTaxonAsync($"{address}/taxa", [1, 103040, 300001])
            .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.Equal("{\"taxa\":[]}", result);
        Assert.Equal("application/json; charset=utf-8", contentType);
        Assert.Equal("{\"TaxonIds\":[1,103040,300001]}", requestBody);
    }
}
