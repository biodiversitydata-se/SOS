using SOS.Api.Contracts.Checklists.Responses;
using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ChecklistsEndpoints;

[Collection(TestCollection.Name)]
public class GetChecklistByIdTests : ElasticsearchTestBase
{
    public GetChecklistByIdTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TestGetChecklistById()
    {
        // Arrange
        const int Id = 123456;
        const string eventId = $"urn:lsid:artportalen.se:Checklist:123456";
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedChecklists()
            .TheFirst(1).With(p => p.Id = Id)
            .Build();
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.GetAsync($"/checklists?id={eventId}", TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ChecklistDto>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert        
        result!.Event.EventId.Should().Be(eventId);
        result.Id.Should().Be(eventId);
    }
}
