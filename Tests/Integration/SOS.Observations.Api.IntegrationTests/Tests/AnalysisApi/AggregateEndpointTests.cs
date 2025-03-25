using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos.Search;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class AggregateEndpointTests : TestBase
{
    public AggregateEndpointTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task AggregationEndpoint_ReturnsExpectedAggregation_WhenAggregatingByYear()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(60).IsInDateSpan("2002-02-01T00:00:00", "2002-02-28T23:59:59")
             .TheNext(20).IsInDateSpan("2003-03-01T00:00:00", "2003-03-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateAnalysisApiClient();
        var searchFilter = new SearchFilterInternalDto
        {
            
        };

        // Act
        var response = await apiClient.PostAsync($"/internal/aggregation?aggregationField=event.startYear", JsonContent.Create(searchFilter));        
        var result = await response.Content.ReadFromJsonAsync<PagedAggregationResultDto<AggregationItemDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Records.Should().BeEquivalentTo(
            new List<AggregationItemDto>
            {
                new AggregationItemDto
                {
                    AggregationField = "2000",
                    Count = 20                    
                },
                new AggregationItemDto
                {
                    AggregationField = "2002",
                    Count = 60
                },
                new AggregationItemDto
                {
                    AggregationField = "2003",
                    Count = 20                    
                }
            },
            options => options.Excluding(m => m.UniqueTaxon)
        );
    }

    public class AggregationItemDto
    {
        /// <summary>
        /// Aggregated field
        /// </summary>
        public string AggregationField { get; set; }

        /// <summary>
        /// Document count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Unique taxon count
        /// </summary>
        public int UniqueTaxon { get; set; }

        /// <summary>
        /// Organism quantity
        /// </summary>
        public int OrganismQuantity { get; set; }
    }
}
