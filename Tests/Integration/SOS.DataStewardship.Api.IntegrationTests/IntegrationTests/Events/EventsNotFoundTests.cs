using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.DataStewardship.Api.IntegrationTests.TestData;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests.Events;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsNotFoundTests : TestBase
{
    public EventsNotFoundTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    // ShouldReturnsHttp404NotFoundWhen_EventIdDoesntExistInDatabase()
    // ShouldReturnsHttp404NotFound_When_EventIdDoesntExistInDatabase()
    // Given_EventIdThatDoesntExist_When_GetEventById_Then_Http404NotFound_Is_Returned()
    // EventById_Returns_Http404NotFound_When_EventId_Doesnt_Exist_In_Database
    // EventById_ReturnsHttp404NotFound_GivenEventIdThatDoesntExist

    [Fact]
    public async Task EventById_ReturnsHttp404NotFound_GivenEventIdThatDoesntExist()
    {
        // Arrange
        string eventId = "NonExistingEventId";
        var events = EventsTestData.GetEventTestData();
        await ProcessFixture.AddEventsToElasticsearchAsync(events);

        // Act
        var result = await ApiClient.GetAsync($"datastewardship/events/{eventId}");      

        // Assert
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);        
    }

    [Fact]
    public async Task EventsBySearch_ReturnsEmptyCollection_GivenSearchCriteraWithNoHits()
    {
        // Arrange                        
        await ProcessFixture.AddEventsToElasticsearchAsync(EventsTestData.GetEventTestData());        
        await ProcessFixture.AddObservationsToElasticsearchAsync(ObservationsTestData.GetObservationTestData());
        var searchFilter = new EventsFilter
        {
            DatasetIds = new List<string> { "NonExistingDatasetId" }
        };

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(0);
        pageResult.Records.Should().BeEmpty();
    }


    //private IEnumerable<ObservationEvent> GetEventTestData(string firstEventKey = null, string? firstDatasetKey = null)
    //{
    //    firstEventKey ??= DataHelper.RandomString(3);
    //    firstDatasetKey ??= DataHelper.RandomString(3);

    //    var events = Builder<ObservationEvent>.CreateListOfSize(10)
    //         .TheFirst(1)
    //            .With(m => m.EventId = firstEventKey)
    //            .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
    //            {
    //                Identifier = firstDatasetKey,
    //            })
    //        .TheNext(9)
    //             .With(m => m.EventId = DataHelper.RandomString(3, new[] { firstEventKey }))
    //             .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
    //             {
    //                 Identifier = DataHelper.RandomString(3, new[] { firstDatasetKey }),
    //             })
    //        .Build();

    //    return events;
    //}

    //private IEnumerable<Observation> GetObservationTestData(string firstEventKey = null, string firstDatasetId = null)
    //{
    //    firstEventKey ??= DataHelper.RandomString(3);
    //    firstDatasetId ??= DataHelper.RandomString(3);

    //    var observations = Builder<Observation>.CreateListOfSize(10)
    //         .TheFirst(1)
    //            .With(m => m.Event = new Event
    //            {
    //                EventId = firstEventKey,
    //                StartDate = DateTime.Now,
    //                EndDate = DateTime.Now,
    //            })
    //            .With(m => m.DataStewardshipDatasetId = firstDatasetId)
    //            .With(m => m.DataProviderId = 1)
    //            .With(m => m.ArtportalenInternal = null)
    //            .With(m => m.Sensitive = false)
    //        .TheNext(9)
    //            .With(m => m.Event = new Event
    //            {
    //                EventId = DataHelper.RandomString(3, new[] { firstEventKey }),
    //                StartDate = DateTime.Now,
    //                EndDate = DateTime.Now,
    //            })
    //            .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3, new[] { firstDatasetId }))
    //            .With(m => m.DataProviderId = 1)
    //            .With(m => m.ArtportalenInternal = null)
    //            .With(m => m.Sensitive = false)
    //        .Build();

    //    return observations;
    //}
}