using FluentAssertions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosDataStewardshipClientTests
{
    [Fact]
    public async Task GetDatasetAsync_EncodesIdentifierAndAddsExplicitRole()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.Should().Be("/DataStewardship/datasets/dataset%2Fone%20%26%20two");
            request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("7");
            return JsonResponse("{\"identifier\":\"dataset/one & two\"}");
        });
        var client = new SosDataStewardshipClient(CreateHttpClient(handler));

        var result = await client.GetDatasetAsync(
            "dataset/one & two",
            new SosRoleRequestOptions { RoleId = 7 },
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchDatasetsAsync_EncodesPagingAndSendsJsonFilter()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.Should().Be("/DataStewardship/datasets");
            request.RequestUri.Query.Should().Be(
                "?includeEventIds=true&validateSearchFilter=false&skip=3&take=4");
            request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken).Result
                .Should().Contain("\"taxon\"");
            return JsonResponse("{\"records\":[],\"skip\":3,\"take\":4,\"totalCount\":0}");
        });
        var client = new SosDataStewardshipClient(CreateHttpClient(handler));

        var result = await client.SearchDatasetsAsync(
            new SearchFilterBaseDto { Taxon = new TaxonFilterDto { Ids = [12] } },
            new DataStewardshipDatasetOptions
            {
                IncludeEventIds = true,
                Skip = 3,
                Take = 4
            },
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Records.Should().BeEmpty();
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) => new(handler)
    {
        BaseAddress = new Uri("https://example.test/")
    };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}
