using FizzWare.NBuilder;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using VerbatimProject = SOS.Lib.Models.Verbatim.Artportalen.Project;
using VerbatimProjectParameter = SOS.Lib.Models.Verbatim.Artportalen.ProjectParameter;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class InvasiveSpeciesTreatmentTests : ElasticsearchTestBase
{
    public InvasiveSpeciesTreatmentTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 3)]
    [InlineData(null, 6)]
    public async Task PublicSearch_FiltersByHasInvasiveSpeciesTreatment(bool? hasTreatment, int expectedCount)
    {
        await AddObservations();
        var filter = new SearchFilterDto
        {
            HasInvasiveSpeciesTreatment = hasTreatment,
            Output = TreatmentOutput()
        };

        var result = await Search(filter);

        result.TotalCount.Should().Be(expectedCount);
        result.Records.Should().HaveCount(expectedCount);
        if (hasTreatment.HasValue)
        {
            result.Records.Should().OnlyContain(observation =>
                (observation.Occurrence.InvasiveSpeciesTreatment != null) == hasTreatment.Value);
        }
    }

    [Theory]
    [InlineData(new[] { (int)InvasiveSpeciesTreatmentId.FollowUpChemical }, 1)]
    [InlineData(new[] { (int)InvasiveSpeciesTreatmentId.FollowUpChemical, (int)InvasiveSpeciesTreatmentId.TreatmentAccordingToComment }, 2)]
    [InlineData(new[] { (int)InvasiveSpeciesTreatmentId.TreatmentBiological }, 1)]
    [InlineData(new[] { int.MaxValue }, 0)]
    [InlineData(new int[0], 6)]
    [InlineData(null, 6)]
    public async Task PublicSearch_FiltersByInvasiveSpeciesTreatmentIds(int[]? treatmentIds, int expectedCount)
    {
        await AddObservations();
        var filter = new SearchFilterDto
        {
            InvasiveSpeciesTreatmentIds = treatmentIds,
            Output = TreatmentOutput()
        };

        var result = await Search(filter);

        result.TotalCount.Should().Be(expectedCount);
        result.Records.Should().HaveCount(expectedCount);
        if (treatmentIds?.Length > 0 && expectedCount > 0)
        {
            result.Records.Should().OnlyContain(observation =>
                observation.Occurrence.InvasiveSpeciesTreatment != null &&
                treatmentIds.Contains(observation.Occurrence.InvasiveSpeciesTreatment.Id));
        }
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task PublicSearch_CombinesInvasiveSpeciesTreatmentFilters(bool hasTreatment, int expectedCount)
    {
        await AddObservations();
        var filter = new SearchFilterDto
        {
            HasInvasiveSpeciesTreatment = hasTreatment,
            InvasiveSpeciesTreatmentIds = [(int)InvasiveSpeciesTreatmentId.FollowUpChemical],
            Output = TreatmentOutput()
        };

        var result = await Search(filter);

        result.TotalCount.Should().Be(expectedCount);
        result.Records.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    [InlineData(null, 1)]
    public async Task PublicCount_AppliesInvasiveSpeciesTreatmentFilters(bool? hasTreatment, int expectedCount)
    {
        await AddObservations();
        var filter = new SearchFilterBaseDto
        {
            HasInvasiveSpeciesTreatment = hasTreatment,
            InvasiveSpeciesTreatmentIds = [(int)InvasiveSpeciesTreatmentId.FollowUpChemical]
        };

        var apiClient = TestFixture.CreateApiClient();
        var response = await apiClient.PostAsync("/observations/count", JsonContent.Create(filter), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await response.Content.ReadFromJsonAsync<int>(TestContext.Current.CancellationToken);
        count.Should().Be(expectedCount);
    }

    private async Task<PagedResultDto<Observation>> Search(SearchFilterDto filter)
    {
        var apiClient = TestFixture.CreateApiClient();
        var response = await apiClient.PostAsync("/observations/search", JsonContent.Create(filter), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.Records.Should().NotContain(observation => observation.ArtportalenInternal != null);
        return result;
    }

    private static OutputFilterDto TreatmentOutput() => new()
    {
        Fields = ["Occurrence.OccurrenceId", "Occurrence.InvasiveSpeciesTreatment"]
    };

    private async Task AddObservations()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(observation => observation.Projects = [new VerbatimProject { Id = 1 }])
            .TheNext(1).With(observation => observation.Projects = [new VerbatimProject { Id = 2 }])
            .TheNext(1).With(observation => observation.Projects = null)
            .TheNext(1).With(observation => observation.Projects = [new VerbatimProject
            {
                Id = 5865,
                ProjectParameters =
                [
                    new VerbatimProjectParameter { Id = 1, Name = "Åtgärd", Value = "Uppföljning" },
                    new VerbatimProjectParameter { Id = 2, Name = "Kemisk åtgärd", Value = "Ja" }
                ]
            }])
            .TheNext(1).With(observation => observation.Projects = [new VerbatimProject { Id = 5444 }])
            .TheLast(1).With(observation => observation.Projects = [new VerbatimProject
            {
                Id = 5865,
                ProjectParameters =
                [
                    new VerbatimProjectParameter { Id = 1, Name = "Åtgärd", Value = "Åtgärd" },
                    new VerbatimProjectParameter { Id = 2, Name = "Biologisk åtgärd", Value = "Ja" }
                ]
            }])
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
    }
}
