using FluentAssertions;
using GeoJSON.Text.Geometry;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Checklists.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Input;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class FilterExtensionsTests
{
    [Theory]
    [InlineData(false, "{}")]
    [InlineData(false, "{\"output\":null}")]
    [InlineData(false, "{\"output\":{}}")]
    [InlineData(true, "{}")]
    [InlineData(true, "{\"output\":null}")]
    [InlineData(true, "{\"output\":{}}")]
    public void SearchFilter_UsesMinimumFields_WhenOutputIsMissingNullOrEmpty(bool internalSearch, string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var filter = internalSearch
            ? JsonSerializer.Deserialize<SearchFilterInternalDto>(json, options)!.ToSearchFilterInternal(42, "sv-SE")
            : JsonSerializer.Deserialize<SearchFilterDto>(json, options)!.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE");

        filter.Output.Fields.Should().NotBeNullOrEmpty()
            .And.Contain(new[] { "Taxon.Id", "Taxon.ScientificName", "Occurrence.OccurrenceId", "Event.StartDate" })
            .And.NotContain("Modified");
        filter.Output.ExcludeFields.Should().BeEmpty();
    }

    [Fact]
    public void AggregationAndExportFilters_UseSharedPublicFilterMapping()
    {
        var aggregation = new SearchFilterAggregationDto { Taxon = new TaxonFilterDto { Ids = [103040] } };
        var export = new ExportFilterDto { Taxon = new TaxonFilterDto { Ids = [103040] } };

        var aggregationFilter = aggregation.ToSearchFilter(42, ProtectionFilterDto.Sensitive, "sv-SE");
        var exportFilter = export.ToSearchFilter(42, ProtectionFilterDto.Sensitive, "sv-SE");
        var baseFilter = ((SearchFilterBaseDto)aggregation).ToSearchFilter(42, ProtectionFilterDto.Sensitive, "sv-SE");

        aggregationFilter.Should().BeOfType<SearchFilter>();
        aggregationFilter.Taxa.Ids.Should().Equal(103040);
        aggregationFilter.Should().BeEquivalentTo(baseFilter);
        exportFilter.Should().BeEquivalentTo(baseFilter);
    }

    [Fact]
    public void InternalAggregationFilter_UsesSharedInternalFilterMapping()
    {
        var dto = new SearchFilterAggregationInternalDto
        {
            Taxon = new TaxonFilterDto { Ids = [103040] },
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            ObservedByMe = true
        };

        var filter = dto.ToSearchFilterInternal(42, "sv-SE");
        var baseFilter = ((SearchFilterInternalBaseDto)dto).ToSearchFilterInternal(42, "sv-SE");

        filter.Should().BeOfType<SearchFilterInternal>();
        filter.Taxa.Ids.Should().Equal(103040);
        filter.ExtendedAuthorization.ProtectionFilter.Should().Be(ProtectionFilter.Sensitive);
        filter.ExtendedAuthorization.ObservedByMe.Should().BeTrue();
        filter.Should().BeEquivalentTo(baseFilter);
    }

    [Theory]
    [InlineData(SearchSortOrder.Asc)]
    [InlineData(SearchSortOrder.Desc)]
    public void ToSearchFilter_PreservesQuerySortOrder(SearchSortOrder sortOrder)
    {
        var dto = new SearchFilterDto();

        var filter = dto.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE", "taxon.id", sortOrder);

        var sort = filter.Output.SortOrders.Should().ContainSingle().Which;
        sort.SortBy.Should().Be("taxon.id");
        sort.SortOrder.Should().Be(sortOrder);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToSearchFilter_LeavesSortOrdersUnset_WhenSortByIsOmitted(string? sortBy)
    {
        var dto = new SearchFilterDto();

        dto.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE", sortBy!, SearchSortOrder.Desc)
            .Output.SortOrders.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSearchFilterInternal_UsesQuerySortOnlyWhenOutputHasNoSortOrders(bool hasOutputSort)
    {
        var dto = new SearchFilterInternalDto
        {
            Output = new OutputFilterExtendedDto
            {
                SortOrders = hasOutputSort
                    ? [new SortOrderDto { SortBy = "modified", SortOrder = SearchSortOrder.Desc }]
                    : []
            }
        };

        var filter = dto.ToSearchFilterInternal(42, "sv-SE", "taxon.id", SearchSortOrder.Asc);

        var sort = filter.Output.SortOrders.Should().ContainSingle().Which;
        sort.SortBy.Should().Be(hasOutputSort ? "modified" : "taxon.id");
        sort.SortOrder.Should().Be(hasOutputSort ? SearchSortOrder.Desc : SearchSortOrder.Asc);
    }

    [Fact]
    public void ToSearchFilter_ReusesServerGeometries()
    {
        var geometry = new GeometryFactory().CreatePoint(new Coordinate(16, 62));
        var geographics = new GeographicsFilterInputModel { ServerGeometries = [geometry] };
        var dto = new SearchFilterDto { Geographics = geographics };

        var filter = dto.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE", "taxon.id", SearchSortOrder.Asc);

        filter.Location.Geometries.Geometries.Should().BeSameAs(geographics.ServerGeometries);
        filter.Location.Geometries.Geometries.Should().ContainSingle().Which.Should().BeSameAs(geometry);
    }

    [Fact]
    public void ToSearchFilter_ConvertsGeoJsonGeometry_ForProgrammaticallyCreatedDto()
    {
        var geometry = new GeometryFactory().CreatePoint(new Coordinate(16, 62));
        var dto = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto { Geometries = [new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(62, 16))] }
        };

        var filter = dto.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE");

        filter.Location.Geometries.Geometries.Should().ContainSingle()
            .Which.EqualsExact(geometry).Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SignalAndSearchFilters_UseTheSameGeographicMapping(bool serverInput)
    {
        var geometry = new GeometryFactory().CreatePoint(new Coordinate(16, 62));
        GeographicsFilterDto geographics = serverInput
            ? new GeographicsFilterInputModel { ServerGeometries = [geometry] }
            : new GeographicsFilterDto { Geometries = [new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(62, 16))] };
        geographics.BoundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto { Longitude = 15, Latitude = 63 },
            BottomRight = new LatLonCoordinateDto { Longitude = 18, Latitude = 60 }
        };
        geographics.MaxDistanceFromPoint = 250;
        geographics.ConsiderDisturbanceRadius = true;
        geographics.ConsiderObservationAccuracy = true;
        geographics.ConsiderAuthorizationBuffer = false;
        geographics.MaxAccuracy = 500;
        geographics.IncludeObservationsOutsideSweden = true;
        geographics.LocationIds = ["location-1"];
        geographics.LocationNameFilter = "test*";
        var search = new SearchFilterDto { Geographics = geographics }.ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE");
        var signal = new SignalFilterDto { Geographics = geographics }.ToSearchFilterInternal(42, false);

        signal.Location.Geometries.Should().BeEquivalentTo(search.Location.Geometries,
            options => options.Excluding(filter => filter.Geometries));
        signal.Location.Geometries.Geometries.Should().ContainSingle().Which
            .EqualsExact(search.Location.Geometries.Geometries.Single()).Should().BeTrue();
        if (serverInput) signal.Location.Geometries.Geometries.Should().BeSameAs(((GeographicsFilterInputModel)geographics).ServerGeometries);
        signal.Location.MaxAccuracy.Should().Be(500);
        signal.Location.IncludeObservationsOutsideSweden.Should().BeNull();
        signal.Location.LocationIds.Should().BeNull();
        signal.Location.NameFilter.Should().BeNull();
    }

    [Fact]
    public void SignalAndSearchFilters_PreserveTheirDifferentMissingLocationDefaults()
    {
        var search = new SearchFilterDto().ToSearchFilter(42, ProtectionFilterDto.Public, "sv-SE");
        var signal = new SignalFilterDto().ToSearchFilterInternal(42, false);

        search.Location.Should().BeNull();
        signal.Location.Should().NotBeNull();
        signal.Location.Geometries.Should().BeNull();
    }

    [Fact]
    public void ToTaxonFilter_MapsBaseCriteriaUsedBySignalSearch()
    {
        var dto = new TaxonFilterBaseDto
        {
            Ids = [1],
            IncludeUnderlyingTaxa = true,
            IsInvasiveInSweden = false,
            TaxonListIds = [2]
        };

        var filter = dto.ToTaxonFilter();

        filter.Ids.Should().Equal(1);
        filter.IncludeUnderlyingTaxa.Should().BeTrue();
        filter.IsInvasiveInSweden.Should().BeFalse();
        filter.ListIds.Should().Equal(2);
        filter.TaxonListOperator.Should().Be(TaxonFilter.TaxonListOp.Merge);
    }

    [Fact]
    public void ToTaxonFilter_PreservesDefaultsAndNullCollections_ForBothDtoTypes()
    {
        var full = new TaxonFilterDto().ToTaxonFilter();
        var basic = new TaxonFilterBaseDto().ToTaxonFilter();

        full.Should().BeEquivalentTo(basic);
        full.Ids.Should().BeNull();
        full.ListIds.Should().BeNull();
        full.IncludeUnderlyingTaxa.Should().BeFalse();
    }

    [Theory]
    [InlineData(SightingTypeFilterDto.DoNotShowMerged, SearchFilterBase.SightingTypeFilter.DoNotShowMerged)]
    [InlineData(SightingTypeFilterDto.ShowOnlyMerged, SearchFilterBase.SightingTypeFilter.ShowOnlyMerged)]
    [InlineData(SightingTypeFilterDto.ShowBoth, SearchFilterBase.SightingTypeFilter.ShowBoth)]
    [InlineData(SightingTypeFilterDto.DoNotShowSightingsInMerged, SearchFilterBase.SightingTypeFilter.DoNotShowSightingsInMerged)]
    [InlineData(SightingTypeFilterDto.DoNotShowMergedIncludeReplacementChilds, SearchFilterBase.SightingTypeFilter.DoNotShowMergedIncludeReplacementChilds)]
    [InlineData(SightingTypeFilterDto.ShowChildrenAndReplacements, SearchFilterBase.SightingTypeFilter.ShowChildrenAndReplacements)]
    public void ToSearchFilter_MapsArtportalenTypeFilter(
        SightingTypeFilterDto dtoValue,
        SearchFilterBase.SightingTypeFilter expected)
    {
        var dto = new SearchFilterDto
        {
            Artportalen = new ArtportalenFilterDto
            {
                TypeFilter = dtoValue
            }
        };

        var filter = dto.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");

        filter.TypeFilter.Should().Be(expected);
    }

    [Fact]
    public void ToSearchFilter_UsesExistingDefault_WhenTypeFilterIsOmitted()
    {
        var dto = new SearchFilterDto();

        var filter = dto.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");

        filter.TypeFilter.Should().Be(SearchFilterBase.SightingTypeFilter.DoNotShowMerged);
    }

    [Theory]
    [InlineData(ProtectionFilterDto.Public, ProtectionFilter.Public)]
    [InlineData(ProtectionFilterDto.Sensitive, ProtectionFilter.Sensitive)]
    [InlineData(ProtectionFilterDto.BothPublicAndSensitive, ProtectionFilter.BothPublicAndSensitive)]
    [InlineData(null, ProtectionFilter.Public)]
    public void ContractProtectionFilter_MapsToSearchDomain(
        ProtectionFilterDto? requested,
        ProtectionFilter expected)
    {
        var publicFilter = new SearchFilterDto().ToSearchFilter(42, requested, "sv-SE");
        var internalFilter = new SearchFilterInternalDto { ProtectionFilter = requested }
            .ToSearchFilterInternal(42, "sv-SE");

        publicFilter.ExtendedAuthorization.ProtectionFilter.Should().Be(expected);
        internalFilter.ExtendedAuthorization.ProtectionFilter.Should().Be(expected);
    }

    [Fact]
    public void ToSearchFilterInternal_ExtendedTypeFilterOverridesArtportalenTypeFilter()
    {
        var dto = new SearchFilterInternalDto
        {
            Artportalen = new ArtportalenFilterDto
            {
                TypeFilter = SightingTypeFilterDto.ShowOnlyMerged
            },
            ExtendedFilter = new ExtendedFilterDto
            {
                TypeFilter = SightingTypeFilterDto.ShowBoth
            }
        };

        var filter = dto.ToSearchFilterInternal(0, "sv-SE");

        filter.TypeFilter.Should().Be(SearchFilterBase.SightingTypeFilter.ShowBoth);
    }

    [Fact]
    public void TaxonFilter_MapsAllContractCriteriaToSearchModel()
    {
        var source = new TaxonFilterDto
        {
            Ids = [1],
            IncludeUnderlyingTaxa = true,
            IsInvasiveInSweden = false,
            TaxonListIds = [2],
            RedListCategories = ["VU"],
            TaxonCategories = [17, 18],
            TaxonListOperator = TaxonListOperatorDto.Filter,
            TaxonomyType = TaxonomyType.Artportalen
        };

        var target = source.ToTaxonFilter();

        target.Ids.Should().Equal(1);
        target.IncludeUnderlyingTaxa.Should().BeTrue();
        target.IsInvasiveInSweden.Should().BeFalse();
        target.ListIds.Should().Equal(2);
        target.RedListCategories.Should().Equal("VU");
        target.TaxonCategories.Should().Equal(17, 18);
        target.TaxonListOperator.Should().Be(TaxonFilter.TaxonListOp.Filter);
        target.TaxonomyType.Should().Be(TaxonomyType.Artportalen);
    }

    [Fact]
    public void ContractFilter_DeserializesExistingDateFilterJson()
    {
        const string json = """{"date":{"startDate":"2000-02-01T00:00:00","endDate":"2000-02-29T23:59:59","dateFilterType":"BetweenStartDateAndEndDate"}}""";

        var filter = JsonSerializer.Deserialize<SearchFilterInternalDto>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        filter.Date.Should().NotBeNull();
        filter.Date!.StartDate!.Value.DateTime.Should().Be(new DateTime(2000, 2, 1, 0, 0, 0));
        filter.Date.EndDate!.Value.DateTime.Should().Be(new DateTime(2000, 2, 29, 23, 59, 59));
        filter.Date.DateFilterType.Should().Be(DateFilterTypeDto.BetweenStartDateAndEndDate);
    }

    [Fact]
    public void ToSearchFilterInternal_PreservesNestedDatesAndFilterValues()
    {
        var source = new SearchFilterInternalDto
        {
            Date = new()
            {
                StartDate = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.FromHours(1)),
                EndDate = new DateTimeOffset(2020, 2, 3, 0, 0, 0, TimeSpan.FromHours(1)),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            ExtendedFilter = new()
            {
                ReportedDateFrom = new DateTimeOffset(2021, 3, 4, 0, 0, 0, TimeSpan.FromHours(1)),
                ReportedDateTo = new DateTimeOffset(2021, 4, 5, 0, 0, 0, TimeSpan.FromHours(2))
            },
            ModifiedDate = new()
            {
                From = new DateTimeOffset(2022, 5, 6, 0, 0, 0, TimeSpan.FromHours(2)),
                To = new DateTimeOffset(2022, 6, 7, 0, 0, 0, TimeSpan.FromHours(2))
            },
            Geographics = new()
            {
                BoundingBox = new()
                {
                    TopLeft = new() { Longitude = 13.84, Latitude = 58.40 },
                    BottomRight = new() { Longitude = 13.90, Latitude = 58.35 }
                }
            }
        };

        var target = source.ToSearchFilterInternal(0, "sv-SE");

        target.Date.Should().NotBeNull();
        target.Date.StartDate.Should().Be(source.Date.StartDate!.Value.UtcDateTime);
        target.Date.EndDate.Should().Be(source.Date.EndDate!.Value.UtcDateTime);
        target.ReportedDateFrom.Should().Be(source.ExtendedFilter.ReportedDateFrom!.Value.UtcDateTime);
        target.ReportedDateTo.Should().Be(source.ExtendedFilter.ReportedDateTo!.Value.UtcDateTime);
        target.ModifiedDate!.From.Should().Be(source.ModifiedDate.From!.Value.UtcDateTime);
        target.ModifiedDate.To.Should().Be(source.ModifiedDate.To!.Value.UtcDateTime);
        target.Location!.Geometries!.BoundingBox!.TopLeft!.Longitude.Should().Be(13.84);
        target.Location.Geometries.BoundingBox.BottomRight!.Latitude.Should().Be(58.35);
    }

    [Theory]
    [InlineData("2026-01-01T10:00:00Z")]
    [InlineData("2026-01-01T13:00:00+03:00")]
    [InlineData("2026-01-01T05:00:00-05:00")]
    [InlineData("2026-01-02T00:00:00+14:00")]
    public void SearchDateMapping_PreservesTheInstantAndUtcKind(string timestamp)
    {
        var json = $$"""
            {
              "date": { "startDate": "{{timestamp}}", "endDate": "{{timestamp}}" },
              "modifiedDate": { "from": "{{timestamp}}", "to": "{{timestamp}}" },
              "extendedFilter": { "reportedDateFrom": "{{timestamp}}", "reportedDateTo": "{{timestamp}}" }
            }
            """;
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var publicRequest = JsonSerializer.Deserialize<SearchFilterDto>(json, options)!;
        var internalRequest = JsonSerializer.Deserialize<SearchFilterInternalDto>(json, options)!;
        var expected = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var publicFilter = publicRequest.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");
        var internalFilter = internalRequest.ToSearchFilterInternal(0, "sv-SE");

        DateTime?[] dates =
        [
            publicFilter.Date.StartDate, publicFilter.Date.EndDate,
            publicFilter.ModifiedDate.From, publicFilter.ModifiedDate.To,
            internalFilter.Date.StartDate, internalFilter.Date.EndDate,
            internalFilter.ModifiedDate.From, internalFilter.ModifiedDate.To,
            internalFilter.ReportedDateFrom, internalFilter.ReportedDateTo
        ];
        foreach (var date in dates)
        {
            date.Should().Be(expected);
            date!.Value.Kind.Should().Be(DateTimeKind.Utc);
            date.Value.ToUniversalTime().Should().Be(expected);
        }
    }

    [Fact]
    public void OffsetlessSearchDateMapping_PreservesLocalTimeInterpretation()
    {
        const string json = """{"date":{"startDate":"2026-01-01T10:00:00","endDate":"2026-01-01T11:00:00"}}""";
        var request = JsonSerializer.Deserialize<SearchFilterDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        var filter = request.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");

        filter.Date.StartDate.Should().Be(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Local).ToUniversalTime());
        filter.Date.EndDate.Should().Be(new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Local).ToUniversalTime());
        filter.Date.StartDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        filter.Date.EndDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("invalid", 0)]
    [InlineData("00:15:00", 15)]
    public void TrendFilter_PreservesDateCriteria_WithOrWithoutMinimumEffort(string? minEffortTime, int expectedMinutes)
    {
        var request = new CalculateTrendFilterDto
        {
            Date = new DateFilterDto
            {
                StartDate = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                EndDate = new DateTimeOffset(2026, 1, 2, 13, 0, 0, TimeSpan.FromHours(3)),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate,
                TimeRanges = [TimeRangeDto.Morning, TimeRangeDto.Evening]
            },
            Checklist = minEffortTime is null ? null : new TrendChecklistFilterDto { MinEffortTime = minEffortTime },
            TaxonId = 103026
        };

        var (_, filter) = request.ToSearchFilters();

        filter.Date.Should().NotBeNull();
        filter.Date!.StartDate.Should().Be(request.Date.StartDate.Value.UtcDateTime);
        filter.Date.EndDate.Should().Be(request.Date.EndDate.Value.UtcDateTime);
        filter.Date.StartDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        filter.Date.EndDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        filter.Date.DateFilterType.Should().Be(DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate);
        filter.Date.TimeRanges.Should().Equal(DateFilter.TimeRange.Morning, DateFilter.TimeRange.Evening);
        filter.Date.MinEffortTime.Should().Be(TimeSpan.FromMinutes(expectedMinutes));
        filter.Taxa.Ids.Should().Equal(103026);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00:15:00")]
    public void TrendFilter_WithoutDates_PreservesOptionalMinimumEffort(string? minEffortTime)
    {
        var request = new CalculateTrendFilterDto
        {
            Checklist = minEffortTime is null ? null : new TrendChecklistFilterDto { MinEffortTime = minEffortTime }
        };

        var (_, filter) = request.ToSearchFilters();

        if (minEffortTime is null)
        {
            filter.Date.Should().BeNull();
        }
        else
        {
            filter.Date!.StartDate.Should().BeNull();
            filter.Date.EndDate.Should().BeNull();
            filter.Date.MinEffortTime.Should().Be(TimeSpan.FromMinutes(15));
        }
    }

    [Fact]
    public void SearchFilterDto_PreservesAllOutputFieldSetFromExistingJson()
    {
        const string json = """{"Output":{"FieldSet":3}}""";
        var contract = JsonSerializer.Deserialize<SearchFilterDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        contract.Output.Should().NotBeNull();
        contract.Output!.FieldSet.Should().Be(OutputFieldSet.All);
        contract.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE")
            .Output.Fields.Should().BeNull();
    }

    [Fact]
    public void PublicOutput_SurvivesBackgroundJobSerialization()
    {
        var request = new SearchFilterDto
        {
            Output = new()
            {
                FieldSet = OutputFieldSet.All
            }
        };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
        var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchFilterDto>(json);

        restored!.Output!.FieldSet.Should().Be(OutputFieldSet.All);
    }

    [Fact]
    public void TryGetSearchGeometries_ReturnsFailureForDegeneratePolygon()
    {
        const string json = """
            {"geographics":{"geometries":[{"coordinates":[[[16.98472499942703,62.90148608256228],[16.98472499942703,62.90148608256228]]],"type":"polygon"}]}}
            """;
        using var document = JsonDocument.Parse(json);
        var geometryJson = document.RootElement
            .GetProperty("geographics")
            .GetProperty("geometries")[0]
            .GetRawText();
        var source = new SearchFilterInternalDto
        {
            Geographics = new()
            {
                Geometries =
                [
                    JsonSerializer.Deserialize<IGeometryObject>(geometryJson)!
                ]
            }
        };

        var result = source.Geographics!.TryGetSearchGeometries();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid geometry");
        source.Geographics.HasGeometryConversionError().Should().BeTrue();
    }

}
