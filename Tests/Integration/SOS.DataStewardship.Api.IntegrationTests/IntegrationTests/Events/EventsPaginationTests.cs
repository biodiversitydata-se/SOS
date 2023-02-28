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
        var events = EventsTestData.GetEventTestData();
        var eventIds = events.Select(m => m.EventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventIds);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new EventsFilter();        
        int take = 2;
        var eventModels = new List<EventModel>();

        // Act
        for (int skip=0; skip < observations.Count(); skip += take)
        {
            var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
                $"datastewardship/events?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);
            eventModels.AddRange(pageResult.Records);
        }

        // Assert
        var uniqueEventIds = eventModels.Select(m => m.EventID).Distinct();
        uniqueEventIds.Should().BeEquivalentTo(eventIds);
    }


    [Fact]
    public async Task EventsBySearch_ReturnsCorrectPagingMetadata_GivenValidInput()
    {
        // Arrange                                
        var events = EventsTestData.GetEventTestData();
        var eventIds = events.Select(m => m.EventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventIds);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new EventsFilter();
        int skip = 5;
        int take = 2;

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);
        
        // Assert
        pageResult.TotalCount.Should().Be(events.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(take);
        pageResult.Skip.Should().Be(skip);
    }
    

    [Fact]
    public async Task EventsBySearch_ReturnsNoRecords_GivenOutOfRangeSkipParameter()
    {
        // Arrange        
        var events = EventsTestData.GetEventTestData();
        var eventIds = events.Select(m => m.EventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventIds);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new EventsFilter();
        int skip = events.Count();
        int take = 2;

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(events.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(0);
        pageResult.Skip.Should().Be(skip);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsBadRequest_GivenInvalidSkipAndTake()
    {
        // Arrange        
        var events = EventsTestData.GetEventTestData();
        var eventIds = events.Select(m => m.EventId);
        await ProcessFixture.AddEventsToElasticsearchAsync(events);
        var observations = GetObservationTestData(eventIds);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        
        var searchFilter = new EventsFilter();
        int skipNegative = -1;
        int skipTooLarge = 1000000;
        int skip = 2;
        int take = 2;
        int takeNegative = -1;
        int takeTooLarge = 1000000;

        // Act
        var responseSkipNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/events?skip={skipNegative}&take={take}", searchFilter, jsonSerializerOptions);

        var responseSkipTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/events?skip={skipTooLarge}&take={take}", searchFilter, jsonSerializerOptions);

        var responseTakeNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/events?skip={skip}&take={takeNegative}", searchFilter, jsonSerializerOptions);

        var responseTakeTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/events?skip={skip}&take={takeTooLarge}", searchFilter, jsonSerializerOptions);

        // Assert
        responseSkipNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseSkipTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
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