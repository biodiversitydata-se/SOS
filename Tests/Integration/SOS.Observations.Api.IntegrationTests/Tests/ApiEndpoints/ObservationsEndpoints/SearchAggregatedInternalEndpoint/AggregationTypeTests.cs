﻿using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.SearchAggregatedInternalEndpoint;

[Collection(TestCollection.Name)]
public class AggregationTypeTests : TestBase
{
    public AggregationTypeTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
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
}