using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ChecklistsEndpoints;

[Collection(TestCollection.Name)]
public class CalculateTrendTests : TestBase
{
    public CalculateTrendTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TestCalculateTrend()
    {
        // Arrange
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(100)
            .All()
                .HaveValuesFromPredefinedChecklists()
                .With(m => m.StartDate = DateTime.Now - TimeSpan.FromMinutes(30)) // 30 minutes search effort time.
                .With(m => m.EndDate = DateTime.Now)
            .TheFirst(60) // 60 talgoxe found
                .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { 103026 })
            .TheNext(20) // 20 talgoxe not found
                .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { 102998 })
            .TheNext(20) // didn't look for talgoxe
                .With(m => m.TaxonIds = new List<int>() { 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { })
            .Build();
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);
        var apiClient = TestFixture.CreateApiClient();
        var calculateTrendFilter = new CalculateTrendFilterDto
        {
            Checklist = new TrendChecklistFilterDto
            {
                MinEffortTime = "00:15:00"
            },
            TaxonId = 103026
        };

        // Act
        var response = await apiClient.PostAsync($"/checklists/calculatetrend", JsonContent.Create(calculateTrendFilter));
        var result = await response.Content.ReadFromJsonAsync<TaxonTrendResult>();

        // Assert
        result!.Quotient.Should().BeApproximately(60.0 / 80, 0.0001, "because there are 60 present observations and 20 absent");
        result.NrPresentObservations.Should().Be(60, "because there are 60 checklists matching the search criteria where talgoxe was found");
        result.NrAbsentObservations.Should().Be(20, "because there are 20 checklists matching the search criteria where talgoxe was not found");
        result.NrChecklists.Should().Be(80, "because there are 80 checklists matching the search criteria");
        result.TaxonId.Should().Be(103026);
    }
}