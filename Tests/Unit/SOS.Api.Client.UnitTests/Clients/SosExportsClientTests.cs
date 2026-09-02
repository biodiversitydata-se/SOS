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
