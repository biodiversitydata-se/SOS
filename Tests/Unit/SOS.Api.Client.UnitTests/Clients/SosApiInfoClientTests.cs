using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.UnitTests.Support;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosApiInfoClientTests
{
    [Fact]
    public async Task GetApiInfoAsync_UsesApiInfoEndpoint()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.Should().Be("/api/ApiInfo");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        });
        var client = new SosApiInfoClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        });

        var result = await client.GetApiInfoAsync(TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetApiInfoAsync_ReturnsNullForNoContent()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new SosApiInfoClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        });

        var result = await client.GetApiInfoAsync(TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}
