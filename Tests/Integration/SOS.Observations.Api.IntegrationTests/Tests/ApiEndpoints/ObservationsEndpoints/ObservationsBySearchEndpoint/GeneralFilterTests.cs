using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class GeneralFilterTests : TestBase
{
    public GeneralFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task PublicSearchOperations_AcceptArtportalenTypeFilter()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(3)
                .With(observation => observation.SightingTypeSearchGroupId =
                    (int)SOS.Lib.Enums.Artportalen.SightingTypeSearchGroup.Assessment)
            .TheNext(1)
                .With(observation => observation.SightingTypeSearchGroupId =
                    (int)SOS.Lib.Enums.Artportalen.SightingTypeSearchGroup.Ordinary)
            .TheLast(1)
                .With(observation => observation.SightingTypeSearchGroupId =
                    (int)SOS.Lib.Enums.Artportalen.SightingTypeSearchGroup.OwnBreedingAssessment)
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        const SightingTypeFilterDto typeFilter =
            SightingTypeFilterDto.ShowOnlyMerged;

        var searchResponse = await apiClient.PostAsync(
            "/observations/search",
            JsonContent.Create(
                new SearchFilterDto
                {
                    Artportalen = new ArtportalenFilterDto { TypeFilter = typeFilter }
                },
                options: JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(
            TestContext.Current.CancellationToken);

        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        searchResult!.TotalCount.Should().Be(3);
        searchResult.Records.Should().OnlyContain(observation => observation.ArtportalenInternal == null);

        var countResponse = await apiClient.PostAsync(
            "/observations/count",
            JsonContent.Create(
                new SearchFilterBaseDto
                {
                    Artportalen = new ArtportalenFilterDto { TypeFilter = typeFilter }
                },
                options: JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var countResult = await countResponse.Content.ReadFromJsonAsync<int>(TestContext.Current.CancellationToken);

        countResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        countResult.Should().Be(3);

        var aggregationResponse = await apiClient.PostAsync(
            "/observations/geogridaggregation?skipCache=true",
            JsonContent.Create(
                new SearchFilterAggregationDto
                {
                    Artportalen = new ArtportalenFilterDto { TypeFilter = typeFilter }
                },
                options: JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var aggregationResult = await aggregationResponse.Content.ReadFromJsonAsync<GeoGridResultDto>(
            TestContext.Current.CancellationToken);

        aggregationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        aggregationResult!.GridCells.Sum(cell => cell.ObservationsCount).Should().Be(3);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByProjectIds()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 1 } })
             .TheNext(1).With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 2 } })
             .TheNext(1).With(o => o.Projects = null)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { ProjectIds = new List<int> { 1 } };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have ProjectId = 1.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPresentObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(6).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = false)
             .TheNext(2).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = false)
             .TheNext(1).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = true)
             .TheNext(1).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(6,
            because: "6 observations added to Elasticsearch are present observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByAbsentObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = false)
             .TheNext(1).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = false)
             .TheNext(1).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = true)
             .TheNext(1).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch are absent observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByVerifiedObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(110)
            .All()
            .HaveValuesFromPredefinedObservations()
            .TheFirst(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReference))

             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueAtReporterHiddenSighting))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NotAbleToValidate))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Rejected))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Verified))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Unvalidated))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { VerificationStatus = StatusVerificationDto.Verified };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(70,
            because: "70 observations added to Elasticsearch have verified status.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByNotVerifiedObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(20)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
             .TheNext(2).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))

             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueAtReporterHiddenSighting))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithReporter))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithValidator))

             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DocumentationRequested))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NeedNotBeValidated))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NotAbleToValidate))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Rejected))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Verified))
             .TheNext(1).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Unvalidated))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { VerificationStatus = StatusVerificationDto.NotVerified };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(12,
            because: "12 observations added to Elasticsearch have not verified status.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByNotUnsureDetermination()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.UnsureDetermination = false)
            .TheNext(2).With(o => o.UnsureDetermination = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            DeterminationFilter = SightingDeterminationFilterDto.NotUnsureDetermination
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have UnsureDetermination = false.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyUnsureDetermination()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.UnsureDetermination = true)
             .TheNext(2).With(o => o.UnsureDetermination = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            DeterminationFilter = SightingDeterminationFilterDto.OnlyUnsureDetermination
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have UnsureDetermination = true.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDontIncludeNotRecovered()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.NotRecovered = false)
             .TheNext(2).With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            NotRecoveredFilter = SightingNotRecoveredFilterDto.DontIncludeNotRecovered
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have NotRecovered = false.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyNotRecovered()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.NotRecovered = true)
             .TheNext(2).With(o => o.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            NotRecoveredFilter = SightingNotRecoveredFilterDto.OnlyNotRecovered
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have NotRecovered = true.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByBirdNestActivity()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(10, 1))
             .TheNext(3).With(o => o.TaxonId = 102986)
                         .With(o => o.Activity = new MetadataWithCategory<int>(10, 2))
             .TheNext(2).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(20, 1))
             .TheNext(1).With(o => o.TaxonId = 102986)
                         .With(o => o.Activity = new MetadataWithCategory<int>(20, 2))
             .TheNext(1).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(11, 1))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { BirdNestActivityLimit = 10 };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(6,
            because: "6 observations added to Elasticsearch have BirdNestActivityLimit <= 10.");
    }
}
