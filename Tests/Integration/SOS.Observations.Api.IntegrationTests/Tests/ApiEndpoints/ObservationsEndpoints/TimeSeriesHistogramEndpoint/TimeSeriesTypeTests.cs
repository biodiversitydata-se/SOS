﻿using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.TimeSeriesHistogramEndpoint;

[Collection(TestCollection.Name)]
public class TimeSeriesTypeTests : ElasticsearchTestBase
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

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
           .All()
               .HaveValuesFromPredefinedObservations()
           .TheFirst(2)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2002, 1, 15))
               .With(p => p.EndDate = new DateTime(2002, 1, 18))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2004, 1, 30))
               .With(p => p.EndDate = new DateTime(2004, 1, 30))
           .TheNext(2)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2006, 1, 1))
               .With(p => p.EndDate = new DateTime(2006, 2, 1))
           .TheLast(2)
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
        var timeSeriesType = TimeSeriesTypeDto.Year;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/timeserieshistogram?timeSeriesType={timeSeriesType}", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSeriesHistogramResultDto>>(options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(11);
        result!.FirstOrDefault(r => r.Period == 2006)?.Taxa.Should().Be(2);
        result.Should().BeEquivalentTo(new List<TimeSeriesHistogramResultDto> {
            new TimeSeriesHistogramResultDto { Period = 2010 },
            new TimeSeriesHistogramResultDto { Period = 2009 },
            new TimeSeriesHistogramResultDto { Period = 2008 },
            new TimeSeriesHistogramResultDto { Period = 2007 },
            new TimeSeriesHistogramResultDto { Period = 2006, Observations = 4, Taxa = 2 },
            new TimeSeriesHistogramResultDto { Period = 2005 },
            new TimeSeriesHistogramResultDto { Period = 2004, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2003 },
            new TimeSeriesHistogramResultDto { Period = 2002, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2001 },
            new TimeSeriesHistogramResultDto { Period = 2000, Observations = 2, Taxa = 1 }
        }, options => options.Excluding(m => m.Quantity).Excluding(m => m.Type));
    }

    [Fact]
    public async Task WeeklyHistogram_UsesTheSwedishOffsetOfEachObservation()
    {
        var verbatim = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations().Build();
        var observations = ProcessFixture.ProcessObservations(verbatim);
        // Sunday evening UTC is still Sunday in winter, but Monday in summer.
        observations[0].Event = new Event(new DateTime(2026, 1, 18, 22, 30, 0, DateTimeKind.Utc));
        observations[1].Event = new Event(new DateTime(2026, 7, 19, 22, 30, 0, DateTimeKind.Utc));
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        using var client = TestFixture.CreateApiClient();
        var filter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTimeOffset.Parse("2026-01-01T00:00:00+01:00"),
                EndDate = DateTimeOffset.Parse("2026-12-31T23:59:59+01:00"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            }
        };

        using var response = await client.PostAsync(
            "/observations/internal/searchaggregated?aggregationType=SightingsPerWeek",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var occupiedWeeks = json.RootElement.GetProperty("records").EnumerateArray()
            .Where(record => record.GetProperty("docCount").GetInt64() > 0)
            .Select(record => record.GetProperty("date").GetDateTime().ToString("yyyy-MM-dd"))
            .ToArray();
        occupiedWeeks.Should().BeEquivalentTo(["2026-01-12", "2026-07-20"]);
    }

    [Fact]
    public async Task YearHistogram_PreservesCalendarYearAtBothBoundaries()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = 100011)
            .Build();
        var observations = ProcessFixture.ProcessObservations(verbatimObservations);
        observations[0].Event = new Event(new DateTime(2000, 1, 1, 0, 30, 0, DateTimeKind.Local));
        observations[1].Event = new Event(new DateTime(2000, 12, 31, 23, 30, 0, DateTimeKind.Local));
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var start = new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local));
        var end = new DateTimeOffset(new DateTime(2000, 12, 31, 23, 59, 59, DateTimeKind.Local));
        using var apiClient = TestFixture.CreateApiClient();

        foreach (var sendUtcDates in new[] { false, true })
        {
            var filter = new SearchFilterAggregationInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = sendUtcDates ? start.ToUniversalTime() : start,
                    EndDate = sendUtcDates ? end.ToUniversalTime() : end,
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                }
            };
            using var response = await apiClient.PostAsync("/observations/internal/timeserieshistogram?timeSeriesType=Year",
                JsonContent.Create(filter), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"sendUtcDates={sendUtcDates}");
            using var scope = new AssertionScope($"sendUtcDates={sendUtcDates}");
            var result = await response.Content.ReadFromJsonAsync<List<TimeSeriesHistogramResultDto>>(
                JsonSerializerOptions, TestContext.Current.CancellationToken);
            var year = result.Should().ContainSingle().Which;
            year.Period.Should().Be(2000);
            year.Observations.Should().Be(2);
            year.Taxa.Should().Be(1);
        }
    }

    [Fact]
    public async Task YearHistogram_WithoutObservations_ReturnsOnlyRequestedCalendarYears()
    {
        var start = new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local));
        var end = new DateTimeOffset(new DateTime(2001, 12, 31, 23, 59, 59, DateTimeKind.Local));
        using var apiClient = TestFixture.CreateApiClient();

        foreach (var sendUtcDates in new[] { false, true })
        {
            var filter = new SearchFilterAggregationInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = sendUtcDates ? start.ToUniversalTime() : start,
                    EndDate = sendUtcDates ? end.ToUniversalTime() : end,
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                }
            };
            using var response = await apiClient.PostAsync("/observations/internal/timeserieshistogram?timeSeriesType=Year",
                JsonContent.Create(filter), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"sendUtcDates={sendUtcDates}");
            using var scope = new AssertionScope($"sendUtcDates={sendUtcDates}");
            var result = await response.Content.ReadFromJsonAsync<List<TimeSeriesHistogramResultDto>>(
                JsonSerializerOptions, TestContext.Current.CancellationToken);
            result!.Select(year => year.Period).Should().Equal(2001, 2000);
            result.Should().OnlyContain(year => year.Observations == 0 && year.Taxa == 0);
        }
    }

    [Fact]
    public async Task TestTimeSeriesHistogramPerYearWithoutDateFilter()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
           .All()
               .HaveValuesFromPredefinedObservations()
           .TheFirst(2)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2002, 1, 15))
               .With(p => p.EndDate = new DateTime(2002, 1, 18))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2004, 1, 30))
               .With(p => p.EndDate = new DateTime(2004, 1, 30))
           .TheNext(2)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2006, 1, 1))
               .With(p => p.EndDate = new DateTime(2006, 2, 1))
           .TheLast(2)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2006, 4, 1))
               .With(p => p.EndDate = new DateTime(2006, 4, 15))
           .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();

        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100011, 100012, 100016, 100017 }
            }
        };
        var timeSeriesType = TimeSeriesTypeDto.Year;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/timeserieshistogram?timeSeriesType={timeSeriesType}", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSeriesHistogramResultDto>>(options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(7); //Only from year of first sighting to last, 2000-2006
        result!.FirstOrDefault(r => r.Period == 2006)?.Taxa.Should().Be(2);
        result.Should().BeEquivalentTo(new List<TimeSeriesHistogramResultDto> {
            new TimeSeriesHistogramResultDto { Period = 2006, Observations = 4, Taxa = 2 },
            new TimeSeriesHistogramResultDto { Period = 2005 },
            new TimeSeriesHistogramResultDto { Period = 2004, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2003 },
            new TimeSeriesHistogramResultDto { Period = 2002, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2001 },
            new TimeSeriesHistogramResultDto { Period = 2000, Observations = 2, Taxa = 1 }
        }, options => options.Excluding(m => m.Quantity).Excluding(m => m.Type));
    }

    [Fact]
    public async Task TestTimeSeriesHistogramPerMonth()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(12)
           .All()
               .HaveValuesFromPredefinedObservations()
           .TheFirst(2)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 2, 15))
               .With(p => p.EndDate = new DateTime(2000, 2, 18))
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 3, 30))
               .With(p => p.EndDate = new DateTime(2000, 3, 30))
           .TheNext(2)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2000, 6, 1))
               .With(p => p.EndDate = new DateTime(2000, 6, 1))
          .TheNext(2)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2001, 6, 1))
               .With(p => p.EndDate = new DateTime(2001, 6, 1))
           .TheLast(2)
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
        var timeSeriesType = TimeSeriesTypeDto.Month;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/timeserieshistogram?timeSeriesType={timeSeriesType}", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSeriesHistogramResultDto>>(options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(12);
        result!.FirstOrDefault(r => r.Period == 6)?.Taxa.Should().Be(2);
        result.Should().BeEquivalentTo(new List<TimeSeriesHistogramResultDto> {
            new TimeSeriesHistogramResultDto { Period = 1, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 2, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 3, Observations = 2, Taxa = 1 },
            new TimeSeriesHistogramResultDto { Period = 4 },
            new TimeSeriesHistogramResultDto { Period = 5 },
            new TimeSeriesHistogramResultDto { Period = 6, Observations = 6, Taxa = 2 },
            new TimeSeriesHistogramResultDto { Period = 7 },
            new TimeSeriesHistogramResultDto { Period = 8 },
            new TimeSeriesHistogramResultDto { Period = 9 },
            new TimeSeriesHistogramResultDto { Period = 10 },
            new TimeSeriesHistogramResultDto { Period = 11 },
            new TimeSeriesHistogramResultDto { Period = 12 }
        }, options => options.Excluding(m => m.Quantity).Excluding(m => m.Type));
        result!.FirstOrDefault(r => r.Period == 6)?.Observations.Should().Be(6);
    }
}
