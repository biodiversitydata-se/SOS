using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Lookups.Responses;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosLookupClientTests
{
    [Fact]
    public async Task GetAreasAsync_BuildsQuery()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Contain("/areas?");
            request.RequestUri.Query.Should().Contain("skip=5");
            request.RequestUri.Query.Should().Contain("take=10");
            request.RequestUri.Query.Should().Contain("areaTypes=Province");
            request.RequestUri.Query.Should().Contain("searchString=uppsala");
            return CreateJsonResponse("""
            {
              "skip": 5,
              "take": 10,
              "totalCount": 1,
              "records": [{ "areaType": "Province", "featureId": "SE-1", "name": "Area one" }]
            }
            """);
        });
        var client = new SosAreasClient(CreateHttpClient(handler));

        var result = await client.GetAreasAsync(
            new AreasOptions
            {
                Skip = 5,
                Take = 10,
                AreaTypes = [AreaTypeDto.Province],
                SearchString = "uppsala"
            },
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<PagedResultDto<AreaBaseDto>>();
        result!.Skip.Should().Be(5);
        result.Take.Should().Be(10);
        result.TotalCount.Should().Be(1);
        result.Records.Should().ContainSingle().Which.FeatureId.Should().Be("SE-1");
    }

    [Fact]
    public async Task GetProjectsByIdsAsync_PostsIdsAndMapsNoContentToEmptyList()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.PathAndQuery.Should().Be("/projects");
            ReadRequestBody(request).Should().Be("[1,2]");
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        var client = new SosProjectsClient(CreateHttpClient(handler));

        var result = await client.GetProjectsByIdsAsync(
            [1, 2],
            TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVocabularyByIdAsync_BuildsRoute()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Be("/vocabularies/SensitivityCategory");
            return CreateJsonResponse("""{"values":[]}""");
        });
        var client = new SosVocabulariesClient(CreateHttpClient(handler));

        var result = await client.GetVocabularyByIdAsync(
            VocabularyIdDto.SensitivityCategory,
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDataProvidersAsync_BuildsRepeatedCategoryQuery()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.PathAndQuery.Should().Contain("/dataproviders?");
            request.RequestUri.Query.Should().Contain("cultureCode=en-GB");
            request.RequestUri.Query.Should().Contain("includeProvidersWithNoObservations=true");
            request.RequestUri.Query.Should().Contain("categories=Atlas");
            return CreateJsonResponse("""[]""");
        });
        var client = new SosDataProvidersClient(CreateHttpClient(handler));

        var result = await client.GetDataProvidersAsync(
            new DataProviderOptions
            {
                CultureCode = "en-GB",
                IncludeProvidersWithNoObservations = true,
                Categories = [DataProviderCategory.Atlas]
            },
            TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
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
