using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventTests : TestBase
{
    public EventTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }    

    [Fact]
    public async Task Get_EventById_Success()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        string identifier = "Abc";
        var events = Builder<ObservationEvent>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.EventId = "Abc")
            .Build();
        await ProcessFixture.AddEventsToElasticsearchAsync(events);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var ev = await ApiClient.GetFromJsonAsync<EventModel>($"datastewardship/events/{identifier}", jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        ev.Should().NotBeNull();
        ev.EventID.Should().Be("Abc");
    }

    [Fact]
    public async Task Post_EventBySearch_Success()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        var events = Builder<ObservationEvent>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.EventId = "Abc")
                .With(m => m.Dataset = new Lib.Models.Processed.DataStewardship.Event.EventDataset
                {
                    Identifier = "Cde",
                })
            .Build();
        await ProcessFixture.AddEventsToElasticsearchAsync(events);

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
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var searchFilter = new EventsFilter { 
            DatasetList = new List<string> { "Cde" } 
        };

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var pageResult = await ApiClient.GetFromJsonPostAsync<PagedResult<EventModel>, EventsFilter>($"datastewardship/events", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        pageResult.Records.First().EventID.Should().Be("Abc");
        pageResult.Records.First().Dataset.Identifier.Should().Be("Cde");
    }
}