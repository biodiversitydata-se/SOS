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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GeometryGetSendsArgumentsAndValidatorAndHandles304(bool notModified)
    {
        const string etag = "W/\"geometry-v1\"";
        const string body = "{\"type\":\"Feature\",\"geometry\":null,\"properties\":{}}";
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsoluteUri.Should().Be("https://observations.example.test/api/areas/County/a%20b/geometry");
            request.Headers.Accept.Select(a => a.MediaType).Should().Contain("application/geo+json");
            request.Headers.IfNoneMatch.Single().ToString().Should().Be(etag);
            var response = new HttpResponseMessage(notModified ? HttpStatusCode.NotModified : HttpStatusCode.OK)
            {
                Content = new StringContent(notModified ? "" : body, Encoding.UTF8, "application/geo+json")
            };
            response.Headers.ETag = System.Net.Http.Headers.EntityTagHeaderValue.Parse(etag);
            return response;
        });
        var client = new SosAreasClient(new HttpClient(handler) { BaseAddress = new Uri("https://observations.example.test/api/") });

        var result = await client.GetAreaGeometryAsync(AreaTypeDto.County, "a b", etag, TestContext.Current.CancellationToken);

        result.NotModified.Should().Be(notModified);
        result.ETag.Should().Be(etag);
        result.GeoJson.Should().Be(notModified ? null : body);
    }

    [Fact]
    public async Task GeometryGetWithoutValidatorFetchesBodyAndReportsMissingAreas()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.IfNoneMatch.Should().BeEmpty();
            return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Area not found") };
        });
        var client = new SosAreasClient(new HttpClient(handler) { BaseAddress = new Uri("https://observations.example.test/") });
        var action = () => client.GetAreaGeometryAsync(AreaTypeDto.County, "missing", cancellationToken: TestContext.Current.CancellationToken);
        await action.Should().ThrowAsync<SOS.Api.Client.Exceptions.SosApiException>();
    }

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
