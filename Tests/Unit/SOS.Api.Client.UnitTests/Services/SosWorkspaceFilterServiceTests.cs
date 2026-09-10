using FluentAssertions;
using GeoJSON.Text.Geometry;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Services;
using SOS.Api.Client.Workspaces;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Workspaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosWorkspaceFilterServiceTests
{
    [Fact]
    public async Task CreateSearchFilterAsync_AppliesWorkspaceDefaults()
    {
        var service = CreateService();

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition(),
            cancellationToken: TestContext.Current.CancellationToken);

        filter.IncludeRealCount.Should().BeTrue();
        filter.DataProvider!.Ids.Should().Equal(-1);
        filter.ProtectionFilter.Should().Be(ProtectionFilterDto.BothPublicAndSensitive);
        filter.OccurrenceStatus.Should().Be(OccurrenceStatusFilterValuesDto.Present);
        filter.ExtendedFilter!.SightingTypeSearchGroupIds.Should().Equal(1, 4, 16, 32, 128);
        filter.Geographics!.IncludeObservationsOutsideSweden.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSearchFilterAsync_UsesFrozenTimeForRelativeDateMode()
    {
        var service = CreateService(new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero));
        var workspace = new SosWorkspaceSearchDefinition
        {
            DateFilter = new WorkspaceDateFilter
            {
                Mode = WorkspaceDateFilterMode.LastFiveYears,
                FilterByObservationDate = true
            }
        };

        var filter = await service.CreateSearchFilterAsync(
            workspace,
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Date!.StartDate.Should().Be(
            new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)));
        filter.Date.EndDate.Should().Be(
            new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.FromHours(1)));
    }

    [Theory]
    [InlineData(WorkspaceDateFilterMode.CurrentYear, 2026, 2026)]
    [InlineData(WorkspaceDateFilterMode.PreviousYear, 2025, 2025)]
    [InlineData(WorkspaceDateFilterMode.LastFiveYears, 2021, 2026)]
    [InlineData(WorkspaceDateFilterMode.LastTwentyFiveYears, 2001, 2026)]
    public async Task CreateSearchFilterAsync_GoldenMasterForRelativeDateModes(
        WorkspaceDateFilterMode mode,
        int expectedStartYear,
        int expectedEndYear)
    {
        var service = CreateService(new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero));

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition
            {
                DateFilter = new WorkspaceDateFilter
                {
                    Mode = mode,
                    FilterByObservationDate = true
                }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Date!.StartDate.Should().Be(
            new DateTimeOffset(expectedStartYear, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)));
        filter.Date.EndDate.Should().Be(
            new DateTimeOffset(expectedEndYear, 12, 31, 23, 59, 59, TimeSpan.FromHours(1)));
        filter.Date.DateFilterType.Should().Be(DateFilterTypeDto.BetweenStartDateAndEndDate);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_GoldenMasterForCustomPeriodAcrossYears()
    {
        var service = CreateService(new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero));

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition
            {
                DateFilter = new WorkspaceDateFilter
                {
                    Mode = WorkspaceDateFilterMode.Custom,
                    FilterByObservationDate = true,
                    IgnoreYear = true,
                    IncludePeriodObservations = true,
                    StartDate = new DateTime(2000, 3, 15),
                    EndDate = new DateTime(2000, 10, 20),
                    StartYear = 2020,
                    EndYear = 2022
                }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Date!.StartDate.Should().Be(
            new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.FromHours(1)));
        filter.Date.EndDate.Should().Be(
            new DateTimeOffset(2026, 10, 20, 23, 59, 59, TimeSpan.FromHours(2)));
        filter.Date.DateFilterType.Should().BeNull();
        filter.ExtendedFilter!.UsePeriodForAllYears.Should().BeTrue();
        filter.ExtendedFilter.Years.Should().Equal(2020, 2021, 2022);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_GoldenMasterForReportedDateMode()
    {
        var service = CreateService(new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero));

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition
            {
                DateFilter = new WorkspaceDateFilter
                {
                    Mode = WorkspaceDateFilterMode.Custom,
                    FilterByObservationDate = false,
                    StartDate = new DateTime(2024, 2, 3),
                    EndDate = new DateTime(2025, 4, 5)
                }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Date.Should().BeNull();
        filter.ExtendedFilter!.ReportedDateFrom.Should().Be(
            new DateTimeOffset(2024, 2, 3, 0, 0, 0, TimeSpan.FromHours(1)));
        filter.ExtendedFilter.ReportedDateTo.Should().Be(
            new DateTimeOffset(2025, 4, 5, 0, 0, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public async Task CreateSearchFilterAsync_AppliesPresenceSpontaneousAndRequestOverrides()
    {
        var service = CreateService();
        var workspace = new SosWorkspaceSearchDefinition
        {
            ExcludeObservationsOutsideSweden = true,
            ObservationDetails = new WorkspaceObservationDetails
            {
                Presence = WorkspacePresence.OnlyNotFound,
                Spontaneous = WorkspaceSpontaneous.UnspontaneousOnly,
                HasInvasiveSpeciesTreatment = true,
                FilterOnMaxAccuracy = true,
                MaxAccuracy = 250
            }
        };

        var filter = await service.CreateSearchFilterAsync(
            workspace,
            new WorkspaceFilterOverrides { TaxonIds = [17, 18] },
            TestContext.Current.CancellationToken);

        filter.OccurrenceStatus.Should().Be(OccurrenceStatusFilterValuesDto.Absent);
        filter.ExtendedFilter!.NotPresentFilter.Should().Be(SightingNotPresentFilterDto.OnlyNotPresent);
        filter.ExtendedFilter.UnspontaneousFilter.Should().Be(SightingUnspontaneousFilterDto.Unspontaneous);
        filter.ExtendedFilter.HasInvasiveSpeciesTreatment.Should().BeTrue();
        filter.Geographics!.IncludeObservationsOutsideSweden.Should().BeFalse();
        filter.Geographics.MaxAccuracy.Should().Be(250);
        filter.Taxon!.Ids.Should().Equal(17, 18);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_GoldenMasterForAreasGeometryBoundingBoxAndPointRadius()
    {
        var service = CreateService();
        var workspaceGeometry = new Point(new Position(59.30, 18.10));
        var boundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto { Latitude = 60, Longitude = 17 },
            BottomRight = new LatLonCoordinateDto { Latitude = 58, Longitude = 19 }
        };

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition
            {
                Areas =
                [
                    new WorkspaceAreaSelection
                    {
                        AreaType = nameof(AreaTypeDto.Municipality),
                        FeatureId = "0180"
                    }
                ],
                Geometries = [workspaceGeometry],
                ObservationDetails = new WorkspaceObservationDetails
                {
                    FilterOnMaxAccuracy = true,
                    MaxAccuracy = 125
                }
            },
            new WorkspaceFilterOverrides
            {
                BoundingBox = boundingBox,
                Point = new LatLonCoordinateDto { Latitude = 59.33, Longitude = 18.07 },
                RadiusInMeters = 750
            },
            TestContext.Current.CancellationToken);

        filter.Geographics!.Areas.Should().ContainSingle();
        filter.Geographics.Areas!.Single().AreaType.Should().Be(AreaTypeDto.Municipality);
        filter.Geographics.Areas.Single().FeatureId.Should().Be("0180");
        filter.Geographics.BoundingBox.Should().BeSameAs(boundingBox);
        filter.Geographics.Geometries.Should().HaveCount(2);
        filter.Geographics.Geometries!.First().Should().BeSameAs(workspaceGeometry);
        filter.Geographics.Geometries.Last().Should().BeOfType<Point>();
        var pointJson = JsonNode.Parse(JsonSerializer.Serialize(
            filter.Geographics.Geometries.Last(),
            GoldenMasterJsonOptions))!;
        pointJson["coordinates"]![0]!.GetValue<double>().Should().Be(18.07);
        pointJson["coordinates"]![1]!.GetValue<double>().Should().Be(59.33);
        filter.Geographics.MaxDistanceFromPoint.Should().Be(750);
        filter.Geographics.MaxAccuracy.Should().Be(125);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_UsesSwedishYearAtUtcYearBoundary()
    {
        var service = CreateService(new DateTimeOffset(2025, 12, 31, 23, 30, 0, TimeSpan.Zero));

        var filter = await service.CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition
            {
                DateFilter = new WorkspaceDateFilter
                {
                    Mode = WorkspaceDateFilterMode.Custom,
                    IgnoreYear = true,
                    StartYear = 2025,
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(2000, 12, 31)
                }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        filter.ExtendedFilter!.Years.Should().Equal(2025, 2026);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_DefaultWorkspaceMatchesLegacySemanticJsonGoldenMaster()
    {
        var filter = await CreateService().CreateSearchFilterAsync(
            new SosWorkspaceSearchDefinition(),
            cancellationToken: TestContext.Current.CancellationToken);

        var actual = JsonNode.Parse(JsonSerializer.Serialize(filter, GoldenMasterJsonOptions))!;

        actual["includeRealCount"]!.GetValue<bool>().Should().BeTrue();
        actual["protectionFilter"]!.GetValue<string>().Should().Be("BothPublicAndSensitive");
        actual["occurrenceStatus"]!.GetValue<string>().Should().Be("Present");
        actual["notRecoveredFilter"]!.GetValue<string>().Should().Be("NoFilter");
        actual["determinationFilter"]!.GetValue<string>().Should().Be("NotUnsureDetermination");
        actual["dataProvider"]!["ids"]!.AsArray().Select(node => node!.GetValue<int>())
            .Should().Equal(-1);
        actual["extendedFilter"]!["notPresentFilter"]!.GetValue<string>()
            .Should().Be("DontIncludeNotPresent");
        actual["extendedFilter"]!["unspontaneousFilter"]!.GetValue<string>()
            .Should().Be("NoFilter");
        actual["extendedFilter"]!["sightingTypeSearchGroupIds"]!.AsArray()
            .Select(node => node!.GetValue<int>()).Should().Equal(1, 4, 16, 32, 128);
        actual["geographics"]!["includeObservationsOutsideSweden"]!.GetValue<bool>()
            .Should().BeTrue();
    }

    [Fact]
    public async Task CreateSearchFilterAsync_ResolvesChildrenListsIntersectionAndExclusions()
    {
        var source = new FakeSource
        {
            Descendants = { [10] = [new SosResolvedTaxon(11, 1), new SosResolvedTaxon(12, 2)] },
            Lists = { [20] = [10, 11, 99], [30] = [11] },
            Taxa = { [10] = new SosResolvedTaxon(10, 1) }
        };
        var service = CreateService(source: source);
        var workspace = new SosWorkspaceSearchDefinition
        {
            TaxonSelection = new WorkspaceTaxonSelection
            {
                Taxa =
                [
                    new WorkspaceSelectedTaxon { TaxonId = 10, IncludeChildren = true },
                    new WorkspaceSelectedTaxon { TaxonId = 99, Exclude = true }
                ],
                TaxonCategoryIds = [1],
                TaxonListIds = [20],
                SpeciesGroups = [new WorkspaceSpeciesGroupSelection { TaxonListId = 30, Exclude = true }]
            }
        };

        var filter = await service.CreateSearchFilterAsync(
            workspace,
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Taxon!.Ids.Should().Equal(10);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_ExplicitTaxonIdsDoNotRequireTaxonSource()
    {
        var workspace = new SosWorkspaceSearchDefinition
        {
            TaxonSelection = new WorkspaceTaxonSelection
            {
                Taxa =
                [
                    new WorkspaceSelectedTaxon { TaxonId = 10 },
                    new WorkspaceSelectedTaxon { TaxonId = 20, Exclude = true }
                ]
            }
        };

        var filter = await CreateService().CreateSearchFilterAsync(
            workspace,
            cancellationToken: TestContext.Current.CancellationToken);

        filter.Taxon!.Ids.Should().Equal(10);
    }

    [Fact]
    public async Task CreateSearchFilterAsync_InvalidWorkspaceValueThrowsTypedValidationException()
    {
        var service = CreateService();
        var workspace = new SosWorkspaceSearchDefinition
        {
            ObservationDetails = new WorkspaceObservationDetails
            {
                Presence = (WorkspacePresence)999
            }
        };

        var action = () => service.CreateSearchFilterAsync(
            workspace,
            cancellationToken: TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<SosWorkspaceFilterValidationException>();
    }

    [Fact]
    public void Constructor_MultipleTaxonSourcesFailWithClearConfigurationError()
    {
        var action = () => new SosWorkspaceFilterService(
            TimeProvider.System,
            [new FakeSource(), new FakeSource()]);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only one ISosWorkspaceTaxonSource*");
    }

    private static SosWorkspaceFilterService CreateService(
        DateTimeOffset? now = null,
        ISosWorkspaceTaxonSource? source = null) =>
        new(new FixedTimeProvider(now ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            source is null ? [] : [source]);

    private static readonly JsonSerializerOptions GoldenMasterJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class FakeSource : ISosWorkspaceTaxonSource
    {
        public Dictionary<int, SosResolvedTaxon> Taxa { get; } = [];
        public Dictionary<int, IReadOnlyCollection<SosResolvedTaxon>> Descendants { get; } = [];
        public Dictionary<int, IReadOnlyCollection<int>> Lists { get; } = [];

        public Task<SosResolvedTaxon?> GetTaxonAsync(int taxonId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Taxa.GetValueOrDefault(taxonId));

        public Task<IReadOnlyCollection<SosResolvedTaxon>> GetDescendantsAsync(int taxonId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Descendants.GetValueOrDefault(taxonId) ?? []);

        public Task<IReadOnlyCollection<int>> GetTaxonListIdsAsync(int taxonListId, bool includeChildTaxaForRedlists, CancellationToken cancellationToken = default) =>
            Task.FromResult(Lists.GetValueOrDefault(taxonListId) ?? []);
    }
}
