using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchInternalEndpoint;

[Collection(TestCollection.Name)]
public class OccurrenceStatusTests : TestBase
{    
    public OccurrenceStatusTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TestOccurrenceStatusScenarios()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(70).With(o => o.NotPresent = false).With(o => o.NotRecovered = false) // => isPositiveObs = true
             .TheNext(20).With(o => o.NotPresent = true).With(o => o.NotRecovered = false) // => isPositiveObs = false
             .TheNext(10).With(o => o.NotPresent = false).With(o => o.NotRecovered = true) // => isPositiveObs = false             
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        
        // Scenario 1 - Get only present observations (Noterade)
        var onlyPresentSearchFilter = new SearchFilterInternalDto {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.NoFilter,
            ExtendedFilter = new ExtendedFilterDto {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.DontIncludeNotPresent
            }
        };
        var onlyPresentResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(onlyPresentSearchFilter));
        var onlyPresentResult = await onlyPresentResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();        
        onlyPresentResult!.TotalCount.Should().Be(70);

        // Scenario 2 - Get only not present observations (Ej noterade (inkl. ej återfunna))
        var notPresentSearchFilter = new SearchFilterInternalDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.NoFilter,
            ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
            }
        };
        var notPresentResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(notPresentSearchFilter));
        var notPresentResult = await notPresentResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        notPresentResult!.TotalCount.Should().Be(30);

        // Scenario 3 - Get only not recovered observations (Ej återfunna)
        var notRecoveredSearchFilter = new SearchFilterInternalDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.OnlyNotRecovered,
            ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
            }
        };
        var notRecoveredResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(notRecoveredSearchFilter));
        var notRecoveredResult = await notRecoveredResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        notRecoveredResult!.TotalCount.Should().Be(10);

        // Scenario 4 - Get both present and absent (Alla)
        var allObsSearchFilter = new SearchFilterInternalDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.NoFilter,
            ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
            }
        };
        var allObsResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(allObsSearchFilter));
        var allObsResult = await allObsResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        allObsResult!.TotalCount.Should().Be(100);

        //-----------------------------------------------
        // Scenarios that probably aren't used that much
        //-----------------------------------------------

        // Scenario 5 (Not useful scenario?) - Not present except not recovered (Ej noterade, exkludera ej återfunna)
        var notPresentExceptRecoveredSearchFilter = new SearchFilterInternalDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.DontIncludeNotRecovered,
            ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
            }
        };
        var notPresentExceptRecoveredResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(notPresentExceptRecoveredSearchFilter));
        var notPresentExceptRecoveredResult = await notPresentExceptRecoveredResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        notPresentExceptRecoveredResult!.TotalCount.Should().Be(20);

        // Scenario 6 (Not useful scenario?) - All except not recovered (Alla (exkl. ej återfunna))
        var allExceptNotRecoveredSearchFilter = new SearchFilterInternalDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent,
            NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.DontIncludeNotRecovered,
            ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.IncludeNotPresent
            }
        };
        var allExceptNotRecoveredResponse = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(allExceptNotRecoveredSearchFilter));
        var allExceptNotRecoveredResult = await allExceptNotRecoveredResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        allExceptNotRecoveredResult!.TotalCount.Should().Be(90);
    }
}