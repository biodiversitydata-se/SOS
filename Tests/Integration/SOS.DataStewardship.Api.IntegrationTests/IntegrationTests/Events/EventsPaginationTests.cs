using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.DataStewardship.Api.IntegrationTests.TestData;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests.Events;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsPaginationTests : TestBase
{
    public EventsPaginationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task EventsBySearch_ReturnsAllEvents_WhenPaginatingAllResultSet()
    {
        // Arrange                        
        string eventId = "Abc";
        string datasetId = "Def";
        var events = EventsTestData.GetEventTestData(eventId, datasetId);
        var eventIds = events.Select(m => m.EventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventIds,  eventId, datasetId);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new EventsFilter();        
        int take = 2;
        var eventModels = new List<EventModel>();

        // Act
        for (int skip=0; skip < observations.Count(); skip += take)
        {
            var pageResult = await ApiClient.GetFromJsonPostAsync<PagedResult<EventModel>, EventsFilter>(
                $"datastewardship/events?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);
            eventModels.AddRange(pageResult.Records);
            var uniqueEventIdsSubset = eventModels.Select(m => m.EventID).Distinct();
        }

        // Assert
        var uniqueEventIds = eventModels.Select(m => m.EventID).Distinct();
        uniqueEventIds.Should().BeEquivalentTo(eventIds);
    }


    [Fact]
    public async Task EventsBySearch_ReturnsCorrectPagingMetadata_GivenValidInput()
    {
        // Arrange        

        // Act        

        // Assert
    }
    

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedTodo_GivenInvalidInput()
    {
        // Arrange        

        // Act

        // Assert
    }


    private IEnumerable<ObservationEvent> GetEventTestData(string firstEventKey = null, string? firstDatasetKey = null)
    {
        firstEventKey ??= DataHelper.RandomString(3);
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

    private IEnumerable<Observation> GetObservationTestData(IEnumerable<string> eventIds, string firstEventKey = null, string firstDatasetId = null)
    {
        firstEventKey ??= DataHelper.RandomString(3);
        firstDatasetId ??= DataHelper.RandomString(3);

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

        var eventIdsList = eventIds.ToList();
        for (int i = 0; i < observations.Count; i++)
        {
            observations[i].Event.EventId = eventIdsList[i];
        }

        return observations;
    }
}