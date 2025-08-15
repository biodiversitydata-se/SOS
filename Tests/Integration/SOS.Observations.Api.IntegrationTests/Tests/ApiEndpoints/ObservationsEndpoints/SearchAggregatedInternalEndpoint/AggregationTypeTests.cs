using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.SearchAggregatedInternalEndpoint;

[Collection(TestCollection.Name)]
public class AggregationTypeTests : TestBase
{
    public AggregationTypeTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TestSearchAggregatedInternalSightingsPerWeek48()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
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
            .TheNext(10)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 30))
                .With(p => p.EndDate = new DateTime(2000, 1, 30))
            .TheNext(10)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2001, 1, 30))
                .With(p => p.EndDate = new DateTime(2001, 1, 30))
            .TheNext(20)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 2, 1))
                .With(p => p.EndDate = new DateTime(2000, 2, 1))
            .TheLast(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 15))
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2001-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100012, 100013 }
            }
        };
        var aggregationType = AggregationType.SightingsPerWeek48;

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/searchaggregated?aggregationType={aggregationType}", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Week48Dto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(52);
        result.Records.Should().BeEquivalentTo(_expectedWeek48Records, options => options.WithStrictOrdering().Excluding(x => x.Quantity));
    }    

    [Fact]
    public async Task TestSearchAggregatedInternalSightingsPerWeek()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
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
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 15))
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterAggregationInternalDto
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
        var aggregationType = AggregationType.SightingsPerWeek;

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/searchaggregated?aggregationType={aggregationType}", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<dynamic>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60);
        result!.Records.Count().Should().Be(5);
    }

    [Fact]
    public async Task TestSearchAggregatedInternalSightingsPerYear()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
           .All()
               .HaveValuesFromPredefinedObservations()
           .TheFirst(20)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "1" })
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 15))
               .With(p => p.EndDate = new DateTime(2000, 1, 18))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "2" })
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 30))
               .With(p => p.EndDate = new DateTime(2000, 1, 30))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "3" })
           .TheNext(20)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 2, 1))
           .TheLast(20)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2000, 4, 1))
               .With(p => p.EndDate = new DateTime(2000, 4, 15))
           .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2000-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100011, 100012 }
            }
        };
        var aggregationType = AggregationType.SightingsPerYear;

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/searchaggregated?aggregationType={aggregationType}", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<dynamic>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60);
        result!.Records.Count().Should().Be(1);
    }

    public class Week48Dto
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public long? DocCount { get; set; }
        public double? Quantity { get; set; }
    }

    private List<Week48Dto> _expectedWeek48Records = new List<Week48Dto>
    {
        new Week48Dto
        {
            Year = 2000,
            Week = 1,
            DocCount = 20,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 2,
            DocCount = 20,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 3,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 4,
            DocCount = 10,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 5,
            DocCount = 20,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 6,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 7,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 8,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 9,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 10,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 11,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 12,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 13,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 14,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 15,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 16,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 17,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 18,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 19,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 20,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 21,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 22,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 23,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 24,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 25,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 26,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 27,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 28,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 29,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 30,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 31,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 32,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 33,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 34,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 35,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 36,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 37,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 38,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 39,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 40,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 41,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 42,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 43,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 44,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 45,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 46,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 47,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2000,
            Week = 48,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2001,
            Week = 1,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2001,
            Week = 2,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2001,
            Week = 3,
            DocCount = 0,
        },
        new Week48Dto
        {
            Year = 2001,
            Week = 4,
            DocCount = 10,
        }
    };
}