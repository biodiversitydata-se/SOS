using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for the ObservationsBySearch endpoint with sorting observations scenarios.
/// </summary>
[Collection(TestCollection.Name)]
public class SortingTests : TestBase
{
    public SortingTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingObservationsByTaxonIdAscendingAndDescending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present};

        // Act
        var responseAsc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=asc", JsonContent.Create(searchFilter));
        var resultAsc = await responseAsc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var responseDesc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=desc", JsonContent.Create(searchFilter));
        var resultDesc = await responseDesc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        responseAsc.StatusCode.Should().Be(HttpStatusCode.OK);
        responseDesc.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc!.Records.Select(m => m.Id).Should().BeInAscendingOrder();
        resultDesc!.Records.Select(m => m.Id).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingByModifiedDateWithSameDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                .With(m => m.EditDate = new DateTime(2024, 12, 24))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present, Output = new OutputFilterDto { FieldSet = Lib.Enums.OutputFieldSet.All } };

        // Act - paginate
        var responseAsc1 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=0&take=50", JsonContent.Create(searchFilter));
        var resultAsc1 = await responseAsc1.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var responseAsc2 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=50&take=50", JsonContent.Create(searchFilter));
        var resultAsc2 = await responseAsc2.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        HashSet<string> occurrenceIds = new HashSet<string>();
        foreach (var obs in resultAsc1.Records)
        {
            occurrenceIds.Should().NotContain(obs.Id);
            occurrenceIds.Add(obs.Occurrence.OccurrenceId);
        }
        foreach (var obs in resultAsc2.Records)
        {
            occurrenceIds.Should().NotContain(obs.Id);
            occurrenceIds.Add(obs.Occurrence.OccurrenceId);
        }
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingByModifiedDateWithDifferentDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()                
            .Build();
        DateTime dateTime = new DateTime(2020, 01, 01);
        foreach (var obs in verbatimObservations)
        {
            obs.EditDate = dateTime;
            dateTime += TimeSpan.FromDays(1);
        }
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present, Output = new OutputFilterDto { Fields = ["modified"] } };

        // Act - paginate
        var responseAsc1 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=0&take=50", JsonContent.Create(searchFilter));
        var resultAsc1 = await responseAsc1.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var responseAsc2 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=50&take=50", JsonContent.Create(searchFilter));
        var resultAsc2 = await responseAsc2.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        DateTime? previousDate = null;
        foreach (var obs in resultAsc1.Records.Union(resultAsc2.Records))
        {
            if (previousDate != null)
            {
                obs.Modified.Value.Should().BeOnOrAfter(previousDate.Value);
            }
            previousDate = obs.Modified.Value;
        }
    }
}