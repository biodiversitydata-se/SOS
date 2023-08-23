using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class GeneralFilterTests : TestBase
{
    public GeneralFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByProjectIds()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 1 } })
             .TheNext(20).With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 2 } })
             .TheNext(20).With(o => o.Projects = null)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { ProjectIds = new List<int> { 1 } };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have ProjectId = 1.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPresentObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = false)
             .TheNext(20).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = false)
             .TheNext(10).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = true)
             .TheNext(10).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are present observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByAbsentObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(40).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = false)
             .TheNext(20).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = false)
             .TheNext(20).With(o => o.NotPresent = false)
                         .With(o => o.NotRecovered = true)
             .TheNext(20).With(o => o.NotPresent = true)
                         .With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are absent observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByVerifiedObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All()
            .HaveValuesFromPredefinedObservations()
            .TheFirst(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))

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
        var searchFilter = new SearchFilterDto { VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.Verified };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have verified status.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByNotVerifiedObservations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
             .TheNext(10).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))

             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueAtReporterHiddenSighting))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithReporter))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithValidator))

             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DocumentationRequested))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NeedNotBeValidated))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NotAbleToValidate))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Rejected))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Verified))
             .TheNext(5).With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Unvalidated))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.NotVerified };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have not verified status.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByNotUnsureDetermination()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.UnsureDetermination = false)
            .TheNext(40).With(o => o.UnsureDetermination = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            DeterminationFilter = SearchFilterBaseDto.SightingDeterminationFilterDto.NotUnsureDetermination
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have UnsureDetermination = false.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyUnsureDetermination()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.UnsureDetermination = true)
             .TheNext(40).With(o => o.UnsureDetermination = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            DeterminationFilter = SearchFilterBaseDto.SightingDeterminationFilterDto.OnlyUnsureDetermination
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have UnsureDetermination = true.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDontIncludeNotRecovered()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.NotRecovered = false)
             .TheNext(40).With(o => o.NotRecovered = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.DontIncludeNotRecovered
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have NotRecovered = false.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyNotRecovered()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(o => o.NotRecovered = true)
             .TheNext(40).With(o => o.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.OnlyNotRecovered
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have NotRecovered = true.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByBirdNestActivity()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(30).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(10, 1))
             .TheNext(30).With(o => o.TaxonId = 102986)
                         .With(o => o.Activity = new MetadataWithCategory<int>(10, 2))
             .TheNext(20).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(20, 1))
             .TheNext(10).With(o => o.TaxonId = 102986)
                         .With(o => o.Activity = new MetadataWithCategory<int>(20, 2))
             .TheNext(10).With(o => o.TaxonId = 100102)
                         .With(o => o.Activity = new MetadataWithCategory<int>(11, 1))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { BirdNestActivityLimit = 10 };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have BirdNestActivityLimit <= 10.");
    }
}