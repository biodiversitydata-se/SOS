using System.IO.Compression;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Shared.Api.Extensions;
using Xunit;

namespace SOS.Observations.Api.UnitTests;

public class ResponseCompressionTests
{
    [Theory]
    [InlineData(null, "application/geo+json", "br", true)]
    [InlineData(null, "application/geo+json", "gzip", true)]
    [InlineData(null, "application/json", "br", true)]
    [InlineData(null, "application/json", "gzip", true)]
    [InlineData("application/geo+json", "application/geo+json", "br", true)]
    [InlineData("application/geo+json", "application/geo+json", "gzip", true)]
    [InlineData("application/geo+json", "application/geo+json", "gzip;q=0", false)]
    [InlineData("application/geo+json", "application/geo+json", "identity", false)]
    [InlineData("application/geo+json", "application/json", "br", false)]
    [InlineData("application/geo+json", "application/json", "gzip", false)]
    [InlineData("application/x-sos-test", "application/x-sos-test", "gzip", true)]
    public async Task NegotiatedCompressionRespectsContentTypesAndPreservesBodyAndHeaders(
        string? configuredMimeType, string responseMimeType, string encoding, bool shouldCompress)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSosResponseCompression(mimeTypes: configuredMimeType is null ? null : [configuredMimeType]);
        await using var app = builder.Build();
        // Exercise the HTTPS branch used behind the forwarded-headers middleware too.
        app.Use((context, next) => { context.Request.Scheme = "https"; return next(context); });
        app.UseResponseCompression();
        var json = "{\"type\":\"FeatureCollection\",\"features\":[" +
            string.Join(',', Enumerable.Repeat("{\"type\":\"Feature\",\"properties\":{\"locality\":\"Åker nära sjön\"},\"geometry\":null}", 100)) + "]}";
        app.Run(async context =>
        {
            context.Response.ContentType = $"{responseMimeType}; charset=utf-8";
            context.Response.Headers["X-Feature-Count"] = "100";
            context.Response.Headers["X-AdaptiveSearch-Strategy"] = "LocationsThenGrid";
            await context.Response.WriteAsync(json);
        });
        await app.StartAsync(TestContext.Current.CancellationToken);
        using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.None });
        using var request = new HttpRequestMessage(HttpMethod.Get, app.Urls.Single());
        request.Headers.TryAddWithoutValidation("Accept-Encoding", encoding);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        Assert.Equal(responseMimeType, response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("100", response.Headers.GetValues("X-Feature-Count").Single());
        Assert.Equal("LocationsThenGrid", response.Headers.GetValues("X-AdaptiveSearch-Strategy").Single());
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        if (shouldCompress)
        {
            Assert.Equal(encoding, response.Content.Headers.ContentEncoding.Single());
            Assert.Contains("Accept-Encoding", response.Headers.Vary);
            Assert.True(bytes.Length < System.Text.Encoding.UTF8.GetByteCount(json));
            using var buffer = new MemoryStream(bytes);
            using Stream decompressed = encoding == "br" ? new BrotliStream(buffer, CompressionMode.Decompress) : new GZipStream(buffer, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressed);
            Assert.Equal(json, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
        else
        {
            Assert.Empty(response.Content.Headers.ContentEncoding);
            Assert.Equal(json, System.Text.Encoding.UTF8.GetString(bytes));
        }
    }
}
