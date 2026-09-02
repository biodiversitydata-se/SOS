using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosObservationsClientEndpointTests
{
    [Fact]
    public async Task CountInternalJsonAsync_PostsFilterAndMapsCount()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/count?");
            request.RequestUri.Query.Should().Contain("validateSearchFilter=true");
            request.RequestUri.Query.Should().Contain("sensitiveObservations=true");
            request.Headers.GetValues("X-Authorization-Role-Id").Should().Contain("17");
            request.Headers.GetValues("X-Authorization-Application-Identifier").Should().Contain("app");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            ReadRequestBody(request).Should().Be("""{"taxon":{"ids":[1001]}}""");
            return CreateJsonResponse("42");
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.CountInternalJsonAsync(
            """{"taxon":{"ids":[1001]}}""",
            new CountInternalOptions
            {
                RoleId = 17,
                AuthorizationApplicationIdentifier = "app",
                ValidateSearchFilter = true,
                SensitiveObservations = true
            },
            TestContext.Current.CancellationToken);

        result.Should().Be(42);
    }

    [Fact]
    public async Task CountInternalAsync_SerializesTypedFilter()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            ReadRequestBody(request).Should().Contain("Present");
            return CreateJsonResponse("7");
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.CountInternalAsync(
            new SearchFilterInternalBaseDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            },
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(7);
    }

    [Fact]
    public async Task GetObservationByIdInternalAsync_BuildsEndpointQueryAndReturnsNullForNoContent()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Contain("/observations/internal/urn%3Alsid%3Aartportalen.se%3ASighting%3A1?");
            request.RequestUri.Query.Should().Contain("outputFieldSet=All");
            request.RequestUri.Query.Should().Contain("translationCultureCode=en-GB");
            request.RequestUri.Query.Should().Contain("resolveGeneralizedObservations=true");
            request.Headers.Accept.Select(header => header.MediaType).Should().Contain("application/json");
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        var client = new SosObservationsClient(CreateHttpClient(handler));

        var result = await client.GetObservationByIdInternalAsync(
            "urn:lsid:artportalen.se:Sighting:1",
            new ObservationByIdInternalOptions
            {
                OutputFieldSet = OutputFieldSet.All,
                TranslationCultureCode = "en-GB",
                ResolveGeneralizedObservations = true
            },
            TestContext.Current.CancellationToken);

        result.Should().BeNull();
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
