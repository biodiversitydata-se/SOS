using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Checklist;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ChecklistsEndpoints;

[Collection(TestCollection.Name)]
public class GetChecklistByIdTests : TestBase
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
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedChecklists()
            .TheFirst(1).With(p => p.Id = Id)
            .Build();
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.GetAsync($"/checklists?id={eventId}");
        var result = await response.Content.ReadFromJsonAsync<ChecklistDto>(JsonSerializerOptions);

        // Assert        
        result!.Event.EventId.Should().Be(eventId);
        result.Id.Should().Be(eventId);
    }
}