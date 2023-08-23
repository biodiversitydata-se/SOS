using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsEndpoints.TaxonAggregationEndpoint;

[Collection(TestCollection.Name)]
public class TaxonAggregationTests : TestBase
{
    public TaxonAggregationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TaxonAggregationTest()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
            .TheNext(20)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 1, 15))
                .With(p => p.EndDate = new DateTime(2000, 1, 18))
            .TheNext(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 30))
                .With(p => p.EndDate = new DateTime(2000, 1, 30))
            .TheNext(20)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 2, 1))
                .With(p => p.EndDate = new DateTime(2000, 2, 1))
            .TheLast(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 4, 1))
                .With(p => p.EndDate = new DateTime(2000, 4, 15))
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterAggregationDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2000-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100012, 100013 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/taxonaggregation", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<dynamic>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "");
    }
}