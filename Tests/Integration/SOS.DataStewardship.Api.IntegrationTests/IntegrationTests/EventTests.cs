using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventTests : TestBase
{
    private IEnumerable<ObservationEvent> GetEventTestData(string firstEventKey, string? firstDatasetKey = null)
    {
        firstDatasetKey ??= DataHelper.RandomString(3);

        var events = Builder<ObservationEvent>.CreateListOfSize(10)
             .TheFirst(1)
                .With(m => m.EventId = firstEventKey)
                .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                {
                    Identifier = firstDatasetKey,
                })
            .TheNext(9)
                 .With(m => m.EventId = DataHelper.RandomString(3, new[] { firstEventKey }))
                 .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                 {
                     Identifier = DataHelper.RandomString(3, new[] { firstDatasetKey }),
                 })
            .Build();

        return events;
    }

    private IEnumerable<Observation> GetObservationTestData(string firstEventKey, string firstDatasetId)
    {
        var observations = Builder<Observation>.CreateListOfSize(10)
             .TheFirst(1)
                .With(m => m.Event = new Event
                {
                    EventId = firstEventKey,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = firstDatasetId)
                .With(m => m.DataProviderId = 1)
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .TheNext(9)
                .With(m => m.Event = new Event
                {
                    EventId = DataHelper.RandomString(3, new[] { firstEventKey }),
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3, new[] { firstDatasetId }))
                .With(m => m.DataProviderId = 1)
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .Build();

        return observations;
    }

    public EventTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_EventById_Success()
    {
        // Arrange
        string eventId = "Abc";
        var events = GetEventTestData(eventId);
        await AddEventsToElasticsearchAsync(events);

        // Act
        var response = await Client.GetFromJsonAsync<EventModel>($"datastewardship/events/{eventId}", jsonSerializerOptions);

        // Assert        
        response.Should().NotBeNull();
        response.EventID.Should().Be(eventId);
    }

    [Fact]
    public async Task Post_EventBySearch_Success()
    {
        // Arrange        
        string eventId = "Abc";
        string datasetId = "Def";
        var events = GetEventTestData(eventId, datasetId);
        await AddEventsToElasticsearchAsync(events);
        
        var observations = GetObservationTestData(eventId, datasetId);
        await AddObservationsToElasticsearchAsync(observations);

        var body = new EventsFilter { DatasetList = new List<string> { datasetId } };

        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/events", body, jsonSerializerOptions);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<EventModel>>(jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().EventID.Should().Be(eventId);
    }
}