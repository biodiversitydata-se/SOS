using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Status.Requests;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosSystemsStatusInfoClientTests
{
    [Fact]
    public async Task GetProcessInfoAsync_ReturnsMetadataProcessInformation()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Be("/Systems/ProcessInformation?active=True");
            return CreateStringResponse(
                HttpStatusCode.OK,
                """
                {
                  "id": "observation-1",
                  "publicCount": 42,
                  "protectedCount": 3,
                  "start": "2026-08-19T08:00:00+02:00",
                  "end": "2026-08-19T09:00:00+02:00",
                  "status": "Success",
                  "metadataInfo": [
                    {
                      "id": "metadata-1",
                      "publicCount": 7,
                      "protectedCount": 0,
                      "start": "2026-08-19T08:00:00+02:00",
                      "end": "2026-08-19T08:05:00+02:00",
                      "status": "Success",
                      "metadataInfo": [],
                      "providersInfo": []
                    }
                  ],
                  "providersInfo": [
                    {
                      "dataProviderId": 1,
                      "dataProviderIdentifier": "artportalen",
                      "latestIncrementalStatus": "Success",
                      "processStart": "2026-08-19T08:00:00+02:00",
                      "processStatus": "Success"
                    }
                  ]
                }
                """,
                "application/json");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var result = await client.GetProcessInfoAsync(
            active: true,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Id.Should().Be("observation-1");
        result.MetadataInfo.Should().ContainSingle(info => info.Id == "metadata-1");
        result.ProvidersInfo.Should().ContainSingle(provider =>
            provider.DataProviderIdentifier == "artportalen" &&
            provider.LatestIncrementalStatus == "Success");
    }

    [Fact]
    public async Task GetMongoDbProcessInfoAsync_CallsProcessMongoDbEndpoint()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Be("/systems/processmongodb");
            return CreateStringResponse(HttpStatusCode.OK, """[{ "id": "observation-1" }]""", "application/json");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var result = await client.GetMongoDbProcessInfoAsync(TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Should().ContainSingle(info => info.Id == "observation-1");
    }

    [Fact]
    public async Task GetTaxonRelationsDiagramAsync_BuildsEndpointQueryAndAcceptHeaders()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return CreateStringResponse(HttpStatusCode.OK, "graph TD", "text/plain");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        await client.GetTaxonRelationsDiagramAsync(
            [1000103, 2000200],
            TaxonRelationsTreeIterationMode.OnlyChildren,
            includeSecondaryRelations: false,
            diagramFormat: DiagramFormat.GraphViz,
            translationCultureCode: "en-US",
            cancellationToken: TestContext.Current.CancellationToken);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.PathAndQuery.Should().Be(
            "/systems/TaxonRelationsDiagram?taxonIds=1000103&taxonIds=2000200&treeIterationMode=OnlyChildren&includeSecondaryRelations=false&diagramFormat=GraphViz&translationCultureCode=en-US");
        capturedRequest.Headers.Accept.Select(header => header.MediaType).Should().Equal("text/plain", "application/json");
    }

    [Fact]
    public async Task GetTaxonRelationsDiagramAsync_ReturnsUnwrappedDiagram_WhenApiReturnsJsonString()
    {
        var diagram = "graph TD\r\n    node_1000103(\"Psophus<br/>[Släkte]<br/>1000103\")";
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/systems/TaxonRelationsDiagram?");
            return CreateStringResponse(HttpStatusCode.OK, JsonSerializer.Serialize(diagram), "application/json");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var result = await client.GetTaxonRelationsDiagramAsync(
            [1000103],
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(diagram);
    }

    [Fact]
    public async Task GetTaxonRelationsDiagramAsync_ReturnsRawDiagram_WhenApiReturnsPlainText()
    {
        var diagram = "graph TD\r\n    node_1000103(\"Psophus<br/>[Släkte]<br/>1000103\")";
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/systems/TaxonRelationsDiagram?");
            return CreateStringResponse(HttpStatusCode.OK, diagram, "text/plain");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var result = await client.GetTaxonRelationsDiagramAsync(
            [1000103],
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(diagram);
    }

    [Fact]
    public async Task GetTaxonRelationsDiagramAsync_ThrowsSosApiException_WhenApiReturnsBadRequest()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/systems/TaxonRelationsDiagram?");
            return CreateStringResponse(HttpStatusCode.BadRequest, "Unknown taxon id.", "text/plain");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var act = () => client.GetTaxonRelationsDiagramAsync(
            [int.MaxValue],
            cancellationToken: TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.ResponseBody.Should().Be("Unknown taxon id.");
    }

    [Fact]
    public async Task GetTaxonRelationsDiagramAsync_PreservesJsonErrorBody_WhenBadRequestIsJsonString()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.RequestUri!.PathAndQuery.Should().Contain("/systems/TaxonRelationsDiagram?");
            return CreateStringResponse(
                HttpStatusCode.BadRequest,
                JsonSerializer.Serialize("Unknown taxon id."),
                "application/json");
        });
        var client = new SosSystemsStatusInfoClient(CreateHttpClient(handler));

        var act = () => client.GetTaxonRelationsDiagramAsync(
            [int.MaxValue],
            cancellationToken: TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<SosApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.ResponseBody.Should().Be(JsonSerializer.Serialize("Unknown taxon id."));
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sos.example")
        };
    }

    private static HttpResponseMessage CreateStringResponse(
        HttpStatusCode statusCode,
        string content,
        string mediaType)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, mediaType)
        };
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
}
