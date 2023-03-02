using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventTests : TestBase
{
    public EventTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task Get_EventById_Success()
    {
        // Arrange
        string eventId = "Abc";
        var events = GetEventTestData(eventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);

        // Act
        var ev = await ApiClient.GetFromJsonAsync<EventModel>($"datastewardship/events/{eventId}", jsonSerializerOptions);

        // Assert        
        ev.Should().NotBeNull();
        ev.EventID.Should().Be(eventId);
    }

    [Fact]
    public async Task Post_EventBySearch_Success()
    {
        // Arrange        
        string eventId = "Abc";
        string datasetId = "Def";
        var events = GetEventTestData(eventId, datasetId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventId, datasetId);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new EventsFilter
        {
            DatasetIds = new List<string> { datasetId }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", searchFilter, jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().EventID.Should().Be(eventId);
    }

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
}