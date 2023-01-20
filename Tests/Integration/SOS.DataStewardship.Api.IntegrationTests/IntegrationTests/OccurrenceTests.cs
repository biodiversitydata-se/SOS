using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrenceTests : TestBase
{
    public OccurrenceTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

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

    [Fact]
    public async Task Get_OccurrenceById_Success()
    {
        // Arrange
        string identifier = "Efg";
        var observations = GetObservationTestData(identifier);
        await AddObservationsToElasticsearchAsync(observations);

        // Act
        var response = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/{identifier}", jsonSerializerOptions);

        // Assert        
        response.Should().NotBeNull();
        response.OccurrenceID.Should().Be(identifier);
    }

    [Fact]
    public async Task Post_OccurrencesBySearch_Success()
    {
        // Arrange data
        string identifier = "Efg";
        var observations = GetObservationTestData(identifier);
        await AddObservationsToElasticsearchAsync(observations);

        var body = new OccurrenceFilter { DatasetIds = new List<string> { "Cde" } };

        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/occurrences", body, jsonSerializerOptions);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<OccurrenceModel>>(jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().OccurrenceID.Should().Be(identifier);
    }
}