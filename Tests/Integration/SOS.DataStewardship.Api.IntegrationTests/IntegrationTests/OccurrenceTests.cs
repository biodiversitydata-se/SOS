using SOS.DataStewardship.Api.Models;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrenceTests : TestBase
{
    public OccurrenceTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_OccurrenceById_Success()
    {
        // Arrange
        string identifier = "Efg";
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
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .Build();
        await AddObservationsToElasticsearchAsync(observations);

        // Act
        var response = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/{identifier}", jsonSerializerOptions);

        // Assert        
        response.Should().NotBeNull();
        response.OccurrenceID.Should().Be("Efg");
    }

    [Fact]
    public async Task Post_OccurrencesBySearch_Success()
    {
        // Arrange data                
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
                .With(m => m.ArtportalenInternal = null)
                .With(m => m.Sensitive = false)
            .Build();
        await AddObservationsToElasticsearchAsync(observations);

        var body = new OccurrenceFilter { DatasetIds = new List<string> { "Cde" } };

        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/occurrences", body, jsonSerializerOptions);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<OccurrenceModel>>(jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().OccurrenceID.Should().Be("Efg");
        pageResult.Records.First().DatasetIdentifier.Should().Be("Cde");
    }
}