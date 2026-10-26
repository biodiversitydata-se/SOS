using Moq;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.Managers.Interfaces;
using System.Text;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.DataProvidersEndpoints;

[Collection(TestCollection.Name)]
[Trait("Category", "ReadOnlyIntegrationTest")]
public class DataProviderEmlEndpointTests(TestFixture fixture)
{
    [Fact]
    public async Task UpperCaseJsonAcceptHeader_ReturnsJson()
    {
        var manager = new Mock<IDataProviderManager>();
        manager.Setup(value => value.GetEmlFileAsync(99))
            .ReturnsAsync(Encoding.UTF8.GetBytes("<eml><title>Test data</title></eml>"));
        using var client = fixture.CreateApiClientWithReplacedService(manager.Object);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/DataProviders/99/EML");
        request.Headers.TryAddWithoutValidation("Accept", "APPLICATION/JSON");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("eml").GetProperty("title").GetString().Should().Be("Test data");
    }
}
