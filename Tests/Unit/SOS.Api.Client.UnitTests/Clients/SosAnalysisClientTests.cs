using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Geo;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosAnalysisClientTests
{
    [Fact]
    public async Task AggregateAreasAsync_PreservesAreaAndCoordinateQueryValues()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.AbsolutePath.Should().Be("/internal/areas/aggregation");
            request.RequestUri.Query.Should().Contain("areaType=County");
            request.RequestUri.Query.Should().Contain("coordinateSys=3035");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"type":"FeatureCollection","features":[]}""", Encoding.UTF8, "application/geo+json")
            };
        });
        var client = CreateClient(handler);

        await client.AggregateAreasAsync(
            new SearchFilterAggregationInternalDto(),
            new AreaAggregationOptions
            {
                AreaType = AreaTypeAggregate.County,
                CoordinateSystem = CoordinateSys.ETRS89_LAEA_Europe
            },
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AggregateAsync_PreservesBaseAddressPathPrefix()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.AbsolutePath.Should().Be("/gateway/internal/aggregation");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"records\":[]}", Encoding.UTF8, "application/json")
            };
        });
        var client = new SosAnalysisClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://analysis.example.test/gateway/")
        });

        await client.AggregateAsync(
            new SearchFilterAggregationInternalDto(),
            new FieldAggregationOptions { AggregationField = "taxon.id" },
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task OrderAooEooAsync_EncodesOptionsAndReturnsJsonString()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(request =>
        {
            calls++;
            request.RequestUri!.AbsolutePath.Should().Be("/internal/order/aoo_eoo");
            request.RequestUri.Query.Should().Contain("alphaValues=1.25");
            request.RequestUri.Query.Should().Contain("metricCoordinateSys=3035");
            request.RequestUri.Query.Should().Contain("coordinateSystem=3035");
            request.RequestUri.Query.Should().Contain("description=Analysis%20%26%20export");
            request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("7");
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("\"analysis-1\"")
            };
        });
        var client = new SosAnalysisClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://analysis.example.test/")
        });

        var result = await client.OrderAooEooAsync(
            new SearchFilterAggregationInternalDto(),
            new AooEooOrderOptions
            {
                Description = "Analysis & export",
                RoleId = 7,
                AlphaValues = [1.25],
                MetricCoordinateSystem = MetricCoordinateSys.ETRS89_LAEA_Europe,
                CoordinateSystem = CoordinateSys.ETRS89_LAEA_Europe
            },
            TestContext.Current.CancellationToken);

        result.Should().Be("analysis-1");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task OrderArticle17Async_EncodesOptionsAndReturnsJsonString()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.AbsolutePath.Should().Be("/internal/order/aoo_eoo/article17");
            request.RequestUri.Query.Should().Contain("maxDistance=25000");
            request.RequestUri.Query.Should().Contain("metricCoordinateSys=3006");
            request.RequestUri.Query.Should().Contain("description=Article%2017%20%26%20export");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("\"article-17-job\"", Encoding.UTF8, "application/json")
            };
        });
        var client = CreateClient(handler);

        var result = await client.OrderArticle17Async(
            new SearchFilterAggregationInternalDto(),
            new Article17OrderOptions
            {
                Description = "Article 17 & export",
                MaxDistance = 25000
            },
            TestContext.Current.CancellationToken);

        result.Should().Be("article-17-job");
    }

    [Fact]
    public async Task OrderAooEooAsync_ThrowsSosApiException_WhenApiReturnsBadRequest()
    {
        const string responseBody = "Invalid analysis order.";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "text/plain")
        });
        var client = CreateClient(handler);

        var act = () => client.OrderAooEooAsync(
            new SearchFilterAggregationInternalDto(),
            new AooEooOrderOptions
            {
                Description = "Invalid order",
                AlphaValues = [1]
            },
            TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.ResponseBody.Should().Be(responseBody);
    }

    private static SosAnalysisClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://analysis.example.test/")
        });
}
