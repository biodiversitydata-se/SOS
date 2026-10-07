using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosObservationAggregationsClientTests
{
    [Fact]
    public async Task CachedCountInternalAsync_PostsTaxonIdsAndDeserializesCounts()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Be("/observations/internal/cachedcount");
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            request.Headers.Accept.Select(header => header.MediaType).Should().Equal("application/json");
            ReadRequestBody(request).Should().Be("[103025]");
            return CreateJsonResponse("""
                [{
                    "taxonId": 103025,
                    "observationCount": 1212,
                    "sumObservationCount": 42424,
                    "provinceCount": 2,
                    "sumProvinceCount": 121,
                    "sumObservationCountByProvinceId": {
                    "1": 100,
                    "2": 21
                    }
                }]
            """);
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.CachedCountInternalAsync(
            new[] { 103025 },
            TestContext.Current.CancellationToken);

        var item = result!.Should().ContainSingle().Subject;
        item.TaxonId.Should().Be(103025);
        item.ObservationCount.Should().Be(1212);
        item.SumObservationCount.Should().Be(42424);
        item.ProvinceCount.Should().Be(2);
        item.SumProvinceCount.Should().Be(121);
        item.SumObservationCountByProvinceId.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["1"] = 100,
            ["2"] = 21
        });
    }

    [Fact]
    public async Task GeoGridAggregationInternalAsync_SerializesTypedFilterAndBuildsQuery()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/geogridaggregation?");
            request.RequestUri.Query.Should().Contain("zoom=8");
            request.RequestUri.Query.Should().Contain("translationCultureCode=sv-SE");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("""{"zoom":8,"gridCellCount":0,"gridCells":[]}""");
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.GeoGridAggregationInternalAsync(
            new SearchFilterAggregationInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present },
            new GeoGridAggregationInternalOptions { Zoom = 8, RoleId = 17, AuthorizationApplicationIdentifier = "test-app" },
            TestContext.Current.CancellationToken);

        result!.Zoom.Should().Be(8);
    }

    [Fact]
    public async Task MetricGridAggregationInternalJsonAsync_MapsMetricResult()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/metricgridaggregation?");
            request.RequestUri.Query.Should().Contain("gridCellSizeInMeters=50000");
            request.RequestUri.Query.Should().Contain("metricCoordinateSys=SWEREF99_TM");
            request.RequestUri.Query.Should().NotContain("outputFormat");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            return CreateJsonResponse("""{"gridCellCount":2,"gridCellSizeInMeters":50000,"gridCells":[]}""");
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.MetricGridAggregationInternalJsonAsync(
            "{}",
            new MetricGridAggregationInternalOptions
            {
                GridCellSizeInMeters = 50000,
                RoleId = 17,
                AuthorizationApplicationIdentifier = "test-app"
            },
            TestContext.Current.CancellationToken);

        result!.GridCellCount.Should().Be(2);
        result.GridCellSizeInMeters.Should().Be(50000);
    }

    [Fact]
    public async Task MetricGridAggregationInternalGeoJsonAsync_ReturnsGeoJsonWhenRequested()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/metricgridaggregation?");
            request.RequestUri.Query.Should().Contain("gridCellSizeInMeters=50000");
            request.RequestUri.Query.Should().Contain("metricCoordinateSys=SWEREF99_TM");
            request.RequestUri.Query.Should().Contain("outputFormat=GeoJson");
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/geo+json", "application/json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"type":"FeatureCollection","features":[]}""", Encoding.UTF8, "application/geo+json")
            };
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.MetricGridAggregationInternalGeoJsonAsync(
            "{}",
            new MetricGridAggregationInternalGeoJsonOptions
            {
                GridCellSizeInMeters = 50000,
                RoleId = 17,
                AuthorizationApplicationIdentifier = "test-app"
            },
            TestContext.Current.CancellationToken);

        result.Features.Should().BeEmpty();
    }

    [Fact]
    public async Task TaxonAggregationInternalJsonAsync_MapsPagedResult()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/taxonaggregation?");
            request.RequestUri.Query.Should().Contain("skip=2");
            request.RequestUri.Query.Should().Contain("take=3");
            request.RequestUri.Query.Should().Contain("sumUnderlyingTaxa=true");
            return CreateJsonResponse("""
            {
              "skip": 2,
              "take": 3,
              "totalCount": 10,
              "records": [{ "taxonId": 1001, "observationCount": 5 }]
            }
            """);
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.TaxonAggregationInternalJsonAsync(
            "{}",
            new TaxonAggregationInternalOptions
            {
                Skip = 2,
                Take = 3,
                SumUnderlyingTaxa = true,
                RoleId = 17,
                AuthorizationApplicationIdentifier = "test-app"
            },
            TestContext.Current.CancellationToken);

        result!.TotalCount.Should().Be(10);
        result.Records.Should().ContainSingle().Which.TaxonId.Should().Be(1001);
    }

    [Fact]
    public async Task TaxonSumAggregationInternalAsync_MapsPagedResult()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/taxonsumaggregation?");
            request.RequestUri.Query.Should().Contain("skip=2");
            request.RequestUri.Query.Should().Contain("take=3");
            request.RequestUri.Query.Should().Contain("sortBy=SumObservationCount");
            request.RequestUri.Query.Should().Contain("sortOrder=asc");
            return CreateJsonResponse("""
                {
                  "skip": 2,
                  "take": 3,
                  "totalCount": 10,
                  "records": [{
                      "taxonId": 103025,
                      "observationCount": 1212,
                      "sumObservationCount": 42424,
                      "provinceCount": 2,
                      "sumProvinceCount": 121,
                      "sumObservationCountByProvinceId": {
                        "1": 100,
                        "2": 21
                      }
                  }]
                }
            """);
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.TaxonSumAggregationInternalAsync(
            new TaxonFilterDto(),
            new TaxonSumAggregationInternalOptions
            {
                Skip = 2,
                Take = 3,
                SortBy = "SumObservationCount",
                SortOrder = "asc",
            },
            TestContext.Current.CancellationToken);

        result!.TotalCount.Should().Be(10);
        result.Records.Should().ContainSingle().Which.TaxonId.Should().Be(103025);
    }

    [Fact]
    public async Task TaxonSumAggregationInternalJsonAsync_MapsPagedResult()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/taxonsumaggregation?");
            request.RequestUri.Query.Should().Contain("skip=2");
            request.RequestUri.Query.Should().Contain("take=3");
            request.RequestUri.Query.Should().Contain("sortBy=SumObservationCount");
            request.RequestUri.Query.Should().Contain("sortOrder=asc");
            return CreateJsonResponse("""
                {
                  "skip": 2,
                  "take": 3,
                  "totalCount": 10,
                  "records": [{
                      "taxonId": 103025,
                      "observationCount": 1212,
                      "sumObservationCount": 42424,
                      "provinceCount": 2,
                      "sumProvinceCount": 121,
                      "sumObservationCountByProvinceId": {
                        "1": 100,
                        "2": 21
                      }
                  }]
                }
            """);
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.TaxonSumAggregationInternalJsonAsync(
            "{}",
            new TaxonSumAggregationInternalOptions
            {
                Skip = 2,
                Take = 3,
                SortBy = "SumObservationCount",
                SortOrder = "asc",
            },
            TestContext.Current.CancellationToken);

        result!.TotalCount.Should().Be(10);
        result.Records.Should().ContainSingle().Which.TaxonId.Should().Be(103025);
    }

    [Fact]
    public async Task TaxonAreaAggregationInternalAsync_BuildsQueryAndMapsTaxonDictionary()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.Should().EndWith("/observations/internal/taxonareaaggregation");
            request.RequestUri.Query.Should().Contain("areaType=County");
            request.RequestUri.Query.Should().Contain("sumTaxaTree=true");
            request.RequestUri.Query.Should().Contain("pruneTaxaTree=true");
            request.RequestUri.Query.Should().NotContain("createFile");
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("""
            {
              "1001": {
                "observationCount": 5,
                "sumObservationCount": 8,
                "observationCountByAreaFeatureId": { "01": 5 },
                "sumObservationCountByAreaFeatureId": { "01": 8 }
              }
            }
            """);
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.TaxonAreaAggregationInternalAsync(
            new SearchFilterAggregationInternalDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            },
            new TaxonAreaAggregationInternalOptions
            {
                AreaType = AreaTypeAggregate.County,
                SumTaxaTree = true,
                PruneTaxaTree = true,
                RoleId = 17,
                AuthorizationApplicationIdentifier = "test-app"
            },
            TestContext.Current.CancellationToken);

        result[1001].ObservationCount.Should().Be(5);
        result[1001].SumObservationCountByAreaFeatureId!["01"].Should().Be(8);
    }


    [Fact]
    public async Task TimeSeriesHistogramInternalJsonAsync_MapsResults()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            AssertAuthorizationHeaders(request);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/timeserieshistogram?");
            request.RequestUri.Query.Should().Contain("timeSeriesType=Month");
            return CreateJsonResponse("""[{ "type": "Month", "period": 6, "observations": 4, "quantity": 7, "taxa": 2 }]""");
        });
        var client = new SosObservationAggregationsClient(CreateHttpClient(handler));

        var result = await client.TimeSeriesHistogramInternalJsonAsync(
            "{}",
            new TimeSeriesHistogramInternalOptions { TimeSeriesType = TimeSeriesTypeDto.Month, RoleId = 17, AuthorizationApplicationIdentifier = "test-app" },
            TestContext.Current.CancellationToken);

        result.Should().ContainSingle();
        result[0].Type.Should().Be(TimeSeriesTypeDto.Month);
        result[0].Observations.Should().Be(4);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent, "")]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.OK, "   ")]
    [InlineData(HttpStatusCode.OK, "null")]
    public async Task EmptyResponses_PreserveEndpointSpecificResults(HttpStatusCode statusCode, string body)
    {
        using var httpClient = CreateHttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }));
        var client = new SosObservationAggregationsClient(httpClient);
        var cancellationToken = TestContext.Current.CancellationToken;

        (await client.GeoGridAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken)).Should().BeNull();
        (await client.MetricGridAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken)).Should().BeNull();
        (await client.TaxonAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken)).Should().BeNull();
        (await client.MetricGridAggregationInternalGeoJsonAsync("{}", cancellationToken: cancellationToken))
            .Features.Should().BeEmpty();
        (await client.TimeSeriesHistogramInternalJsonAsync("{}", cancellationToken: cancellationToken)).Should().BeEmpty();

        var action = () => client.TaxonAreaAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken);
        var exception = (await action.Should().ThrowAsync<SosInvalidResponseException>()).Which;
        exception.StatusCode.Should().Be(statusCode);
        exception.RequestUri!.AbsolutePath.Should().Be("/observations/internal/taxonareaaggregation");
        exception.ResponseBodyPreview.Should().BeNull();
    }

    [Theory]
    [InlineData("geogridaggregation")]
    [InlineData("metricgridaggregation")]
    [InlineData("metricgridaggregationgeojson")]
    [InlineData("taxonaggregation")]
    [InlineData("taxonareaaggregation")]
    [InlineData("timeserieshistogram")]
    public async Task AggregationRequest_PreservesHttpErrors(string endpoint)
    {
        const string errorBody = "Invalid filter";
        using var httpClient = CreateHttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorBody)
            }));
        var client = new SosObservationAggregationsClient(httpClient);

        var action = () => CallEndpointAsync(client, endpoint, TestContext.Current.CancellationToken);

        var exception = (await action.Should().ThrowAsync<SosApiException>()).Which;
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.ResponseBody.Should().Be(errorBody);
        exception.RequestUri!.AbsolutePath.Should().Be(EndpointPath(endpoint));
    }

    [Theory]
    [InlineData("geogridaggregation")]
    [InlineData("metricgridaggregation")]
    [InlineData("metricgridaggregationgeojson")]
    [InlineData("taxonaggregation")]
    [InlineData("taxonareaaggregation")]
    [InlineData("timeserieshistogram")]
    public async Task AggregationRequest_PreservesInvalidJsonErrors(string endpoint)
    {
        const string invalidBody = "not JSON";
        using var httpClient = CreateHttpClient(new StubHttpMessageHandler(_ => CreateJsonResponse(invalidBody)));
        var client = new SosObservationAggregationsClient(httpClient);

        var action = () => CallEndpointAsync(client, endpoint, TestContext.Current.CancellationToken);

        var exception = (await action.Should().ThrowAsync<SosInvalidResponseException>()).Which;
        exception.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.ResponseBodyPreview.Should().Be(invalidBody);
        exception.RequestUri!.AbsolutePath.Should().Be(EndpointPath(endpoint));
    }

    [Theory]
    [InlineData("geogridaggregation")]
    [InlineData("metricgridaggregation")]
    [InlineData("metricgridaggregationgeojson")]
    [InlineData("taxonaggregation")]
    [InlineData("taxonareaaggregation")]
    [InlineData("timeserieshistogram")]
    public async Task AggregationRequest_PassesCancellationToHttpClient(string endpoint)
    {
        var handler = new WaitingHttpMessageHandler();
        using var httpClient = CreateHttpClient(handler);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var client = new SosObservationAggregationsClient(httpClient);
        var request = CallEndpointAsync(client, endpoint, cancellation.Token);
        await handler.Started.Task.WaitAsync(TestContext.Current.CancellationToken);

        cancellation.Cancel();

        var action = () => request;
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static void AssertAuthorizationHeaders(HttpRequestMessage request)
    {
        request.Headers.GetValues("X-Authorization-Role-Id").Should().Equal("17");
        request.Headers.GetValues("X-Authorization-Application-Identifier").Should().Equal("test-app");
    }

    private static string EndpointPath(string endpoint) =>
        "/observations/internal/" + (endpoint == "metricgridaggregationgeojson" ? "metricgridaggregation" : endpoint);

    private static Task CallEndpointAsync(
        SosObservationAggregationsClient client,
        string endpoint,
        CancellationToken cancellationToken) => endpoint switch
    {
        "geogridaggregation" => client.GeoGridAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken),
        "metricgridaggregation" => client.MetricGridAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken),
        "metricgridaggregationgeojson" => client.MetricGridAggregationInternalGeoJsonAsync("{}", cancellationToken: cancellationToken),
        "taxonaggregation" => client.TaxonAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken),
        "taxonareaaggregation" => client.TaxonAreaAggregationInternalJsonAsync("{}", cancellationToken: cancellationToken),
        "timeserieshistogram" => client.TimeSeriesHistogramInternalJsonAsync("{}", cancellationToken: cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(endpoint))
    };

    private sealed class WaitingHttpMessageHandler : HttpMessageHandler
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("The request should have been cancelled.");
        }
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sos.example")
        };
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static string ReadRequestBody(HttpRequestMessage request)
    {
        return request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken).GetAwaiter().GetResult();
    }
}
