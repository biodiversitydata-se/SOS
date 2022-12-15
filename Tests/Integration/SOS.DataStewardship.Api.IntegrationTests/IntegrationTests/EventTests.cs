using SOS.DataStewardship.Api.Models;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventTests : TestBase
{
    public EventTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_EventById_Success()
    {
        // Arrange data
        string identifier = "Abc";
        var events = Builder<ObservationEvent>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.EventId = "Abc")                
            .Build();
        await AddEventsToElasticsearchAsync(events);

        // Act
        var response = await Client.GetFromJsonAsync<EventModel>($"datastewardship/events/{identifier}");

        // Assert        
        response.Should().NotBeNull();
        response.EventID.Should().Be("Abc");
    }

    [Fact]
    public async Task Post_EventBySearch_Success()
    {
        // Arrange data        
        var events = Builder<ObservationEvent>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.EventId = "Abc")
                .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                {
                    Identifier = "Cde",
                })
            .Build();
        await AddEventsToElasticsearchAsync(events);
        
        var observations = Builder<Observation>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Event = new Event
                {
                    EventId = "Abc",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = "Cde")
                .With(m => m.DataProviderId = 1)
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .Build();
        await AddObservationsToElasticsearchAsync(observations);

        var body = new EventsFilter { DatasetList = new List<string> { "Cde" } };

        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/events", body);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<EventModel>>();

        // Assert        
        pageResult.Records.First().EventID.Should().Be("Abc");
        pageResult.Records.First().Dataset.Identifier.Should().Be("Cde");
    }
}