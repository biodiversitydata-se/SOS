using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).With(observation => observation.TaxonId = 100013)
            .TheNext(2).With(observation => observation.TaxonId = 100011)
            .TheLast(2).With(observation => observation.TaxonId = 100012)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present};

        // Act
        var responseAsc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=asc", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc = await responseAsc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var responseDesc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=desc", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultDesc = await responseDesc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        responseAsc.StatusCode.Should().Be(HttpStatusCode.OK);
        responseDesc.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc!.Records!.Select(observation => observation.Taxon.Id).Should().Equal(100011, 100011, 100012, 100012, 100013, 100013);
        resultDesc!.Records!.Select(observation => observation.Taxon.Id).Should().Equal(100013, 100013, 100012, 100012, 100011, 100011);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingByModifiedDateWithSameDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
                .With(m => m.EditDate = new DateTime(2024, 12, 24))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present, Output = new OutputFilterDto { FieldSet = OutputFieldSet.All } };

        // Act - paginate
        var responseAsc1 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=0&take=3", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc1 = await responseAsc1.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var responseAsc2 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=3&take=3", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc2 = await responseAsc2.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        responseAsc1.StatusCode.Should().Be(HttpStatusCode.OK);
        responseAsc2.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc1!.Records.Should().HaveCount(3);
        resultAsc2!.Records.Should().HaveCount(3);
        HashSet<string> occurrenceIds = new HashSet<string>();
        foreach (var obs in resultAsc1.Records)
        {
            occurrenceIds.Should().NotContain(obs.Occurrence.OccurrenceId);
            occurrenceIds.Add(obs.Occurrence.OccurrenceId);
        }
        foreach (var obs in resultAsc2.Records)
        {
            occurrenceIds.Should().NotContain(obs.Occurrence.OccurrenceId);
            occurrenceIds.Add(obs.Occurrence.OccurrenceId);
        }
        occurrenceIds.Should().HaveCount(6);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingByModifiedDateWithDifferentDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
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
        var responseAsc1 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=0&take=3", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc1 = await responseAsc1.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var responseAsc2 = await apiClient.PostAsync($"/observations/search?sortBy=modified&sortOrder=asc&skip=3&take=3", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc2 = await responseAsc2.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        responseAsc1.StatusCode.Should().Be(HttpStatusCode.OK);
        responseAsc2.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc1!.Records.Should().HaveCount(3);
        resultAsc2!.Records.Should().HaveCount(3);
        DateTime? previousDate = null;
        foreach (var obs in resultAsc1.Records.Concat(resultAsc2.Records))
        {
            if (previousDate != null)
            {
                obs.Modified!.Value.Should().BeOnOrAfter(previousDate.Value);
            }
            previousDate = obs.Modified!.Value;
        }
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenSortByUsingPascalCase()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).With(observation => observation.TaxonId = 100013)
            .TheNext(2).With(observation => observation.TaxonId = 100011)
            .TheLast(2).With(observation => observation.TaxonId = 100012)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var responseAsc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=asc", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultAsc = await responseAsc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var responseDesc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=desc", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var resultDesc = await responseDesc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        responseAsc.StatusCode.Should().Be(HttpStatusCode.OK);
        responseDesc.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc!.Records!.Select(observation => observation.Taxon.Id).Should().Equal(100011, 100011, 100012, 100012, 100013, 100013);
        resultDesc!.Records!.Select(observation => observation.Taxon.Id).Should().Equal(100013, 100013, 100012, 100012, 100011, 100011);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsBadRequest_WhenSortByPropertyThatDoesntExists()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var responseAsc = await apiClient.PostAsync($"/observations/search?sortBy=Taxn.Id&sortOrder=asc", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        
        // Assert
        responseAsc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
