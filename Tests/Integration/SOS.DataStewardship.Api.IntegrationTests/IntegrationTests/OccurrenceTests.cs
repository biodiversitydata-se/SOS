using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrenceTests : TestBase
{
    public OccurrenceTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }    

    private IEnumerable<Observation> GetObservationTestData(string firstKey)
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        const string identifier = "Efg";
        var observations = Builder<Observation>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Occurrence = new Occurrence
                {
                    OccurrenceId = firstKey,
                })
                .With(m => m.Event = new Event
                {
                    EventId = "Abc",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = "Cde")
                .With(m => m.DataProviderId = 1)                
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var observation = await ApiClient.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/{identifier}", jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        observation.Should().NotBeNull();
        observation.OccurrenceID.Should().Be("Efg");
    }

    [Fact]
    public async Task Post_OccurrencesBySearch_Success()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        var observations = Builder<Observation>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Occurrence = new Occurrence
                {
                    OccurrenceId = "Efg",
                })
                .With(m => m.Event = new Event
                {
                    EventId = "Abc",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = "Cde")
                .With(m => m.DataProviderId = 1)                
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "Cde" } };

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var pageResult = await ApiClient.GetFromJsonPostAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        pageResult.Records.First().OccurrenceID.Should().Be("Efg");
        pageResult.Records.First().DatasetIdentifier.Should().Be("Cde");
    }
}