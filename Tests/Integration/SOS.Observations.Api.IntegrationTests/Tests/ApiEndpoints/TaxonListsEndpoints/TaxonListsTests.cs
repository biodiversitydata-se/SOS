using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.TaxonListsEndpoints;

/// <summary>
/// Integration tests for Taxon lists.
/// </summary>
[Collection(TestCollection.Name)]
public class TaxonListsTests : TestBase
{
    public TaxonListsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    /// <summary>
    /// Requirement from Lst.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetTaxonListsEndpoint_AllTaxonListsContainsParentIdEvenIfItIsNull()
    {
        // Arrange                
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.GetAsync($"/taxonlists");
        var result = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var dictionary in result!)
        {
            dictionary.Should().ContainKey("parentId");
        }
    }
}