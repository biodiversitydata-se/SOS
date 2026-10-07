using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosExportsClientTests
{
    [Fact]
    public async Task OrderInternalAsync_EncodesQueryAddsRoleAndAcceptsPlainTextId()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(request =>
        {
            calls++;
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.Should().Be("/exports/internal/order/Csv");
            request.RequestUri.Query.Should().Contain("description=My%20export%20%26%20more");
            request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("42");
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("job-123", Encoding.UTF8, "text/plain")
            };
        });
        var client = new SosExportsClient(CreateHttpClient(handler));

        var result = await client.OrderInternalAsync(
            new SearchFilterInternalDto(),
            SosExportType.Csv,
            new ExportOrderOptions { Description = "My export & more", RoleId = 42 },
            TestContext.Current.CancellationToken);

        result.Should().Be("job-123");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task DownloadInternalAsync_DisposingContentDisposesUnderlyingHttpContentStream()
    {
        var source = new TrackingStream(Encoding.UTF8.GetBytes("content"));
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Accept.Should().BeEmpty();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(source)
            };
        });
        var client = new SosExportsClient(CreateHttpClient(handler));

        var response = await client.DownloadInternalAsync(
            new SearchFilterInternalDto(),
            SosExportType.Csv,
            cancellationToken: TestContext.Current.CancellationToken);
        await response.Content.DisposeAsync();

        source.WasDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadInternalAsync_DisposesResponseWhenOpeningStreamFails()
    {
        var content = new ThrowingStreamContent();
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        });
        var client = new SosExportsClient(CreateHttpClient(handler));

        var action = () => client.DownloadInternalAsync(
            new SearchFilterInternalDto(),
            SosExportType.Csv,
            cancellationToken: TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<IOException>();
        content.WasDisposed.Should().BeTrue();
    }

    [Theory]
    [InlineData(SosExportType.Csv, false)]
    [InlineData(SosExportType.Csv, true)]
    [InlineData(SosExportType.Excel, false)]
    [InlineData(SosExportType.Excel, true)]
    [InlineData(SosExportType.GeoJson, false)]
    [InlineData(SosExportType.GeoJson, true)]
    [InlineData(SosExportType.DwC, false)]
    [InlineData(SosExportType.DwC, true)]
    public async Task DownloadInternalAsync_PreservesGzipAndFormatSpecificQueryParameters(SosExportType exportType, bool gzip)
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            var query = request.RequestUri!.Query.TrimStart('?').Split('&');
            query.Count(parameter => parameter.StartsWith("gzip=", StringComparison.Ordinal))
                .Should().Be(exportType == SosExportType.DwC ? 0 : 1);
            if (exportType != SosExportType.DwC)
                query.Should().Contain($"gzip={gzip.ToString().ToLowerInvariant()}");

            query.Any(parameter => parameter.StartsWith("flat=", StringComparison.Ordinal))
                .Should().Be(exportType == SosExportType.GeoJson);
            query.Any(parameter => parameter.StartsWith("dynamicProjectDataFields=", StringComparison.Ordinal))
                .Should().Be(exportType == SosExportType.Excel);
            query.Any(parameter => parameter.StartsWith("eventBased=", StringComparison.Ordinal))
                .Should().Be(exportType == SosExportType.DwC);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("content") };
        });
        var client = new SosExportsClient(CreateHttpClient(handler));

        await using var response = await client.DownloadInternalAsync(new SearchFilterInternalDto(), exportType,
            new ExportDownloadOptions { Gzip = gzip }, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetMyExports_UsesDefaultOrExplicitRoleOptions(bool explicitOptions)
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Contains("X-Authorization-Role-Id").Should().Be(explicitOptions);
            request.Headers.Contains("X-Authorization-Application-Identifier").Should().Be(explicitOptions);
            if (explicitOptions)
            {
                request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("42");
                request.Headers.GetValues("X-Authorization-Application-Identifier").Should().ContainSingle("test-app");
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(request.RequestUri!.AbsolutePath.EndsWith("/my", StringComparison.Ordinal)
                    ? "[]" : "{\"id\":\"job-1\"}", Encoding.UTF8, "application/json")
            };
        });
        var client = new SosExportsClient(CreateHttpClient(handler));
        var options = explicitOptions
            ? new SosRoleRequestOptions { RoleId = 42, AuthorizationApplicationIdentifier = "test-app" }
            : null;

        (await client.GetMyExportsAsync(options, TestContext.Current.CancellationToken)).Should().BeEmpty();
        (await client.GetMyExportAsync("job-1", options, TestContext.Current.CancellationToken))!.Id.Should().Be("job-1");
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) => new(handler)
    {
        BaseAddress = new Uri("https://example.test/")
    };

    private sealed class TrackingStream(byte[] bytes) : MemoryStream(bytes)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            WasDisposed = true;
            await base.DisposeAsync();
        }
    }

    private sealed class ThrowingStreamContent : HttpContent
    {
        public bool WasDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new IOException("Unable to open response stream.");

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            throw new IOException("Unable to open response stream.");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }
}
