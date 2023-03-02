using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrenceTests : TestBase
{
    public OccurrenceTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task Get_OccurrenceById_Success()
    {
        // Arrange
        string identifier = "Efg";
        var observations = GetObservationTestData(identifier);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        // Act
        var observation = await ApiClient.GetFromJsonAsync<OccurrenceModel>(
            $"datastewardship/occurrences/{identifier}", jsonSerializerOptions);

        // Assert        
        observation.Should().NotBeNull();
        observation.OccurrenceID.Should().Be(identifier);
    }

    [Fact]
    public async Task Post_OccurrencesBySearch_Success()
    {
        // Arrange
        string identifier = "Efg";
        var observations = GetObservationTestData(identifier);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter
        {
            DatasetIds = new List<string> { "Cde" }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().OccurrenceID.Should().Be(identifier);
    }

    private IEnumerable<Observation> GetObservationTestData(string firstKey)
    {
        var observations = Builder<Observation>.CreateListOfSize(10)
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
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .TheNext(9)
                 .With(m => m.Occurrence = new Occurrence
                 {
                     OccurrenceId = DataHelper.RandomString(3, new[] { firstKey }),
                 })
                .With(m => m.Event = new Event
                {
                    EventId = DataHelper.RandomString(3),
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                })
                .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3))
                .With(m => m.DataProviderId = 1)
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .Build();

        return observations;
    }
}