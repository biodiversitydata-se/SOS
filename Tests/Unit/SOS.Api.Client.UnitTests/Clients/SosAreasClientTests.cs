using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosAreasClientTests
{
    [Fact]
    public async Task GetAreaExportAsync_UsesAnyAcceptProfile()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Accept.Should().BeEmpty();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("area"))
            };
        });
        var client = new SosAreasClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://observations.example.test/")
        });

        var result = await client.GetAreaExportAsync(
            AreaTypeDto.County,
            "1",
            AreaExportFormatDto.Json,
            TestContext.Current.CancellationToken);

        Encoding.UTF8.GetString(result!).Should().Be("area");
    }
}
