using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosObservationsClientTests
{
    [Fact]
    public async Task GetObservationByIdAsync_UsesPublicEndpointAndEncodesOptions()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.Should().EndWith("/observations/id%20with%20space");
            request.RequestUri.Query.Should().Contain("outputFieldSet=AllWithValues");
            request.RequestUri.Query.Should().Contain("translationCultureCode=en-US");
            request.RequestUri.Query.Should().Contain("sensitiveObservations=true");
            return CreateJsonResponse("""{"id":"id with space"}""");
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdAsync(
            "id with space",
            new ObservationByIdOptions
            {
                OutputFieldSet = OutputFieldSet.AllWithValues,
                TranslationCultureCode = "en-US",
                SensitiveObservations = true
            },
            TestContext.Current.CancellationToken);

        result!.Id.Should().Be("id with space");
    }

    [Fact]
    public async Task ObservationsBySearchInternalAsync_SerializesTypedFilter()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/search?");
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("""
            {
              "skip": 0,
              "take": 1,
              "totalCount": 1,
              "records": []
            }
            """);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsBySearchInternalAsync(
            new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present },
            new ObservationSearchOptions { Take = 1 },
            TestContext.Current.CancellationToken);
                
        result.Take.Should().Be(1);
    }

    [Fact]
    public async Task ObservationsBySearchInternalAsync_SerializesIncludeFilterInCamelCase()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            ReadRequestBody(request).Should().Be(
                """{"typeFilter":"DoNotShowMerged","includeFilter":{"occurrenceIds":["occurrence-1"]}}""");
            return CreateJsonResponse("""{"skip":0,"take":0,"totalCount":0,"records":[]}""");
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        await client.ObservationsBySearchInternalAsync(
            new SearchFilterInternalDto
            {
                IncludeFilter = new IncludeFilterDto { OccurrenceIds = ["occurrence-1"] }
            },
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ObservationsBySearchInternalJsonAsync_MapsRecordsPagingAndContentType()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/search?");
            request.RequestUri.Query.Should().Contain("skip=3");
            request.RequestUri.Query.Should().Contain("take=2");
            request.RequestUri.Query.Should().Contain("outputFormat=Json");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            return CreateJsonResponse("""
            {
              "skip": 3,
              "take": 2,
              "totalCount": 11,
              "records": [
                { "id": "7" }
              ]
            }
            """);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsBySearchInternalAsync(
            "{}",
            new ObservationSearchOptions { Skip = 3, Take = 2, OutputFormat = OutputFormatDto.Json },
            TestContext.Current.CancellationToken);
        
        result.Skip.Should().Be(3);
        result.Take.Should().Be(2);
        result.TotalCount.Should().Be(11);
        result.Records.Should().ContainSingle();
        result.Records[0].Id.Should().Be("7");
        result.GeoJson.Should().BeNull();
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task ObservationsBySearchInternalJsonAsync_MapsGeoJsonAndMetadata()
    {
        var geoJson = """{"type":"FeatureCollection","features":[]}""";
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/search?");
            request.RequestUri.Query.Should().Contain("outputFormat=GeoJson");
            request.RequestUri.Query.Should().Contain("includePolygonLocations=true");
            return CreateJsonResponse($$"""
            {
              "skip": 0,
              "take": 100,
              "totalCount": 1,
              "geoJson": {{JsonString(geoJson)}},
              "metadata": {
                "polygonLocations": {
                  "123": {
                    "type": "Polygon"
                  }
                }
              }
            }
            """);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsBySearchInternalAsync(
            "{}",
            new ObservationSearchOptions
            {
                OutputFormat = OutputFormatDto.GeoJson,
                IncludePolygonLocations = true
            },
            TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Records.Should().BeEmpty();
        result.GeoJson.Should().Be(geoJson);
        result.Metadata.Should().NotBeNull();
        //result.Metadata!.Value.GetProperty("polygonLocations")
        //    .GetProperty("123")
        //    .GetProperty("type")
        //    .GetString()
        //    .Should()
        //    .Be("Polygon");
    }

    [Fact]
    public async Task ObservationsByCursorAsync_SerializesTypedFilter()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/searchbycursor?");
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("""
            {
              "nextCursor": "next-page",
              "take": 1,
              "totalCount": 1,
              "records": [
                { "id": "obs-1" }
              ]
            }
            """);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsByCursorAsync(
            new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present },
            new SearchByCursorOptions { Take = 1 },
            TestContext.Current.CancellationToken);

        result.NextCursor.Should().Be("next-page");
        result.Take.Should().Be(1);
        result.TotalCount.Should().Be(1);
        var observation = result.Records!.Should().ContainSingle().Which;
        observation.Id.Should().Be("obs-1");
    }

    [Fact]
    public async Task ObservationsByCursorJsonAsync_BuildsEndpointQueryAndAcceptHeader()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/searchbycursor?");
            request.RequestUri.Query.Should().Contain("take=25");
            request.RequestUri.Query.Should().Contain("sortBy=modified");
            request.RequestUri.Query.Should().Contain("sortOrder=Desc");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            return CreateJsonResponse("""
            {
              "nextCursor": "next-page",
              "take": 25,
              "totalCount": 42,
              "records": []
            }
            """);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsByCursorJsonAsync(
            "{}",
            new SearchByCursorOptions
            {
                Take = 25,
                SortBy = "modified",
                SortOrder = SearchSortOrder.Desc
            },
            TestContext.Current.CancellationToken);

        result.NextCursor.Should().Be("next-page");
        result.Take.Should().Be(25);
        result.TotalCount.Should().Be(42);
    }

    [Fact]
    public async Task ObservationsBySearchInternalJsonAsync_ThrowsInvalidResponseException_WhenResponseBodyCannotBeDeserialized()
    {
        var handler = new StubHttpMessageHandler(_ => CreateJsonResponse("not-json"));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var act = async () => await client.ObservationsBySearchInternalAsync(
            "{}",
            new ObservationSearchOptions(),
            TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosInvalidResponseException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.Which.ResponseBodyPreview.Should().Be("not-json");
        exception.Which.InnerException.Should().BeOfType<JsonException>();
    }

    [Fact]
    public async Task ObservationsBySearchInternalJsonAsync_DeserializesLargeResponseFromNonSeekableStream()
    {
        const int recordCount = 2_000;
        var records = string.Join(",", Enumerable.Range(1, recordCount).Select(index => $$"""{"id":"{{index}}"}"""));
        var json = $$"""{"skip":0,"take":{{recordCount}},"totalCount":{{recordCount}},"records":[{{records}}]}""";
        var handler = new StubHttpMessageHandler(_ => CreateStreamingJsonResponse(json));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.ObservationsBySearchInternalAsync(
            "{}",
            new ObservationSearchOptions(),
            TestContext.Current.CancellationToken);

        result.Records.Should().HaveCount(recordCount);
        result.Records[0].Id.Should().Be("1");
        result.Records[^1].Id.Should().Be(recordCount.ToString());
    }

    [Fact]
    public async Task GetObservationByIdInternalAsync_ReturnsNull_ForNoContent()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetObservationByIdInternalAsync_ReturnsNull_ForExplicitZeroLengthContent()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([])
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("   \t\r\n")]
    [InlineData("null")]
    public async Task GetObservationByIdInternalAsync_ReturnsNull_ForEmptyJsonValue(string responseBody)
    {
        var handler = new StubHttpMessageHandler(_ => CreateStreamingJsonResponse(responseBody));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetObservationByIdInternalAsync_DeserializesValidJson()
    {
        var handler = new StubHttpMessageHandler(_ => CreateStreamingJsonResponse("""{"id":"observation-1"}"""));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be("observation-1");
    }

    [Fact]
    public async Task GetObservationByIdInternalAsync_UsesTruncatedPreview_ForLargeMalformedJsonResponse()
    {
        const int previewSize = 4 * 1024;
        const string truncatedMarker = "\n...[truncated]";
        var responseBody = "{" + new string('x', previewSize + 500);
        var handler = new StubHttpMessageHandler(_ => CreateStreamingJsonResponse(responseBody));
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var act = async () => await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosInvalidResponseException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.OK);
        exception.Which.ResponseBodyPreview.Should().Be(responseBody[..previewSize] + truncatedMarker);
        exception.Which.InnerException.Should().BeOfType<JsonException>();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetObservationByIdInternalAsync_BoundsResponseBodyPreview_ForHttpErrors(
        HttpStatusCode statusCode)
    {
        var responseBody = "error:" + new string('x', 5_000);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "text/plain")
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var act = async () => await client.GetObservationByIdInternalAsync(
            "observation-1",
            cancellationToken: TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosApiException>();
        exception.Which.StatusCode.Should().Be(statusCode);
        exception.Which.ResponseBody.Should().Be(responseBody[..(4 * 1024)]);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsync_SerializesTypedFilter()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/adaptivesearchlocations?");
            ReadRequestBody(request).Should().Contain("Present");
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/geo+json", "application/json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"type":"FeatureCollection","features":[]}""", Encoding.UTF8, "application/geo+json")
            };
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.AdaptiveSearchLocationsInternalAsync(
            new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present },
            new AdaptiveSearchLocationsOptions { ObservationsLimit = 1 },
            TestContext.Current.CancellationToken);

        result.GeoJson.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalJsonAsync_BuildsEndpointQueryAndGeoJsonAcceptHeader()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/adaptivesearchlocations?");
            request.RequestUri.Query.Should().Contain("observationsLimit=10");
            request.RequestUri.Query.Should().Contain("cluster=false");
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/geo+json", "application/json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"type":"FeatureCollection","features":[]}""", Encoding.UTF8, "application/geo+json")
            };
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.AdaptiveSearchLocationsInternalJsonAsync(
            "{}",
            new AdaptiveSearchLocationsOptions
            {
                ObservationsLimit = 10,
                Cluster = false
            },
            TestContext.Current.CancellationToken);

        result.GeoJson.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task AdaptiveSearchInternalAsync_ReturnsJsonBodyWithJsonAcceptHeader()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.Should().EndWith("/observations/internal/adaptivesearch");
            request.RequestUri.Query.Should().Contain("observationsLimit=0");
            request.RequestUri.Query.Should().Contain("includeFilterAreasInResult=true");
            request.RequestUri.Query.Should().Contain("outputFormat=Json");
            request.Headers.Accept.Select(header => header.MediaType).Should().Equal("application/json");
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("""{"resultType":"Observations","resultCount":0}""");
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.AdaptiveSearchInternalAsync(
            new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present },
            new AdaptiveSearchOptions
            {
                ObservationsLimit = 0,
                IncludeFilterAreasInResult = true,
                OutputFormat = OutputFormatDto.Json
            },
            TestContext.Current.CancellationToken);

        result.Content.Should().Contain("Observations");
        result.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task AdaptiveSearchInternalJsonAsync_ReturnsGeoJsonHeaders()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/geo+json", "application/json");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"type":"FeatureCollection","features":[]}""",
                    Encoding.UTF8,
                    "application/geo+json")
            };
            response.Headers.Add("X-Observations-TotalCount", "42");
            response.Headers.Add("X-Result-Count", "8");
            response.Headers.Add("X-Result-Type", "GeoGrid");
            response.Headers.Add("X-Zoom", "6");
            return response;
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.AdaptiveSearchInternalJsonAsync(
            "{}",
            cancellationToken: TestContext.Current.CancellationToken);

        result.ObservationsTotalCount.Should().Be(42);
        result.ResultCount.Should().Be(8);
        result.ResultType.Should().Be("GeoGrid");
        result.Zoom.Should().Be(6);
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

    private static HttpResponseMessage CreateStreamingJsonResponse(string json)
    {
        var content = new StreamContent(new NonSeekableReadStream(Encoding.UTF8.GetBytes(json)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        };
    }

    private static string JsonString(string value)
    {
        return System.Text.Json.JsonSerializer.Serialize(value);
    }

    private static string ReadRequestBody(HttpRequestMessage request)
    {
        return request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken).GetAwaiter().GetResult();
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(send(request));
        }
    }

    private sealed class NonSeekableReadStream(byte[] content) : Stream
    {
        private readonly MemoryStream _innerStream = new(content, writable: false);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            return _innerStream.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return _innerStream.ReadAsync(buffer, cancellationToken);
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            return _innerStream.DisposeAsync();
        }
    }
}
