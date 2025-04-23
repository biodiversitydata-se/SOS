using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.TimeSeriesHistogramEndpoint;

[Collection(TestCollection.Name)]
public class TimeSeriesTypeTests : TestBase
{
    public TimeSeriesTypeTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TestTimeSeriesHistogramPerYear()
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
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2002, 1, 15))
               .With(p => p.EndDate = new DateTime(2002, 1, 18))
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2004, 1, 30))
               .With(p => p.EndDate = new DateTime(2004, 1, 30))
           .TheNext(20)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2006, 1, 1))
               .With(p => p.EndDate = new DateTime(2006, 2, 1))
           .TheLast(20)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2006, 4, 1))
               .With(p => p.EndDate = new DateTime(2006, 4, 15))
           .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2010-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100011, 100012, 100016, 100017 }
            }
        };
        var timeSeriesType = TimeSeriesType.Year;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/timeserieshistogram?timeSeriesType={timeSeriesType}", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSeriesHistogramResultDto>>(options);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(7);
        result!.FirstOrDefault(r => r.Period == 2006)?.Taxa.Should().Be(2);
        result.Should().BeEquivalentTo(new List<TimeSeriesHistogramResultDto> {
            new TimeSeriesHistogramResultDto { Period = 2000, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2001 },
            new TimeSeriesHistogramResultDto { Period = 2002, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2003 },
            new TimeSeriesHistogramResultDto { Period = 2004, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2005 },
            new TimeSeriesHistogramResultDto { Period = 2006, Observations = 40, Taxa = 2 }
        }, options => options.Excluding(m => m.Quantity).Excluding(m => m.Type));
    }

    [Fact]
    public async Task TestTimeSeriesHistogramPerMonth()
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
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 2, 15))
               .With(p => p.EndDate = new DateTime(2000, 2, 18))
           .TheNext(20)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 3, 30))
               .With(p => p.EndDate = new DateTime(2000, 3, 30))
           .TheNext(20)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2000, 6, 1))
               .With(p => p.EndDate = new DateTime(2000, 6, 1))
          .TheNext(20)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2001, 6, 1))
               .With(p => p.EndDate = new DateTime(2001, 6, 1))
           .TheLast(20)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2000, 6, 1))
               .With(p => p.EndDate = new DateTime(2000, 6, 15))
           .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2002-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100011, 100012, 100016, 100017 }
            }
        };
        var timeSeriesType = TimeSeriesType.Month;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/timeserieshistogram?timeSeriesType={timeSeriesType}", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSeriesHistogramResultDto>>(options);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(12);
        result!.FirstOrDefault(r => r.Period == 6)?.Taxa.Should().Be(2);
        result.Should().BeEquivalentTo(new List<TimeSeriesHistogramResultDto> {
            new TimeSeriesHistogramResultDto { Period = 1, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 3, Observations = 20, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 4 },
            new TimeSeriesHistogramResultDto { Period = 5 },
            new TimeSeriesHistogramResultDto { Period = 6, Observations = 40, Taxa = 2 },
            new TimeSeriesHistogramResultDto { Period = 7 },
            new TimeSeriesHistogramResultDto { Period = 8 },
            new TimeSeriesHistogramResultDto { Period = 9 },
            new TimeSeriesHistogramResultDto { Period = 10 },
            new TimeSeriesHistogramResultDto { Period = 11 },
            new TimeSeriesHistogramResultDto { Period = 12 }
        }, options => options.Excluding(m => m.Quantity).Excluding(m => m.Type));
        //result!.FirstOrDefault(r => r.Period == 6)?.Observations.Should().Be(60); 
    }
}