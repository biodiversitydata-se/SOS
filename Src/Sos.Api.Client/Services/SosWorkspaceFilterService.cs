using GeoJSON.Text.Geometry;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Workspaces;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Workspaces;

namespace SOS.Api.Client.Services;

/// <summary>
/// Converts application-independent workspace settings into SOS search filters.
/// </summary>
public sealed class SosWorkspaceFilterService : ISosWorkspaceFilterService
{
    private static readonly int[] NoDataProviderSelection = [-1];
    private static readonly int[] NonMatchingTaxonSelection = [-1];

    private readonly TimeProvider _timeProvider;
    private readonly WorkspaceTaxonSelectionResolver _taxonSelectionResolver;

    /// <summary>Creates the service with a clock and an optional application taxon source.</summary>
    public SosWorkspaceFilterService(
        TimeProvider timeProvider,
        IEnumerable<ISosWorkspaceTaxonSource> taxonSources)
    {
        _timeProvider = timeProvider;
        _taxonSelectionResolver = new WorkspaceTaxonSelectionResolver(taxonSources);
    }

    /// <inheritdoc />
    public Task<SearchFilterInternalDto> CreateSearchFilterAsync(
        SosWorkspaceDocument workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default) =>
        CreateSearchFilterAsync(GetSearchDefinition(workspace), overrides, cancellationToken);

    /// <inheritdoc />
    public Task<SearchFilterAggregationInternalDto> CreateAggregationFilterAsync(
        SosWorkspaceDocument workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default) =>
        CreateAggregationFilterAsync(GetSearchDefinition(workspace), overrides, cancellationToken);

    private static SosWorkspaceSearchDefinition GetSearchDefinition(SosWorkspaceDocument workspace)
    {
        SosWorkspaceDocuments.Validate(workspace);
        return workspace.Search;
    }

    /// <inheritdoc />
    public async Task<SearchFilterInternalDto> CreateSearchFilterAsync(
        SosWorkspaceSearchDefinition workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        var filter = new SearchFilterInternalDto
        {
            IncludeRealCount = true,
            Output = overrides?.Output
        };
        await PopulateFilterAsync(filter, workspace, overrides, cancellationToken);
        return filter;
    }

    /// <inheritdoc />
    public async Task<SearchFilterAggregationInternalDto> CreateAggregationFilterAsync(
        SosWorkspaceSearchDefinition workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        var filter = new SearchFilterAggregationInternalDto();
        await PopulateFilterAsync(filter, workspace, overrides, cancellationToken);
        return filter;
    }

    private async Task PopulateFilterAsync(
        SearchFilterInternalBaseDto filter,
        SosWorkspaceSearchDefinition workspace,
        WorkspaceFilterOverrides? overrides,
        CancellationToken cancellationToken)
    {
        SosWorkspaceDocuments.ValidateSearch(workspace);
        var details = workspace.ObservationDetails ?? new WorkspaceObservationDetails();
        var dateFilter = workspace.DateFilter ?? new WorkspaceDateFilter();

        filter.ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive;
        filter.DataProvider = new DataProviderFilterDto
        {
            Ids = workspace.DataProviderIds?.Count > 0
                ? workspace.DataProviderIds.ToArray()
                : NoDataProviderSelection
        };

        if (workspace.DataStewardshipDatasetIdentifiers?.Count > 0)
        {
            filter.DataStewardship = new DataStewardshipFilterDto
            {
                DatasetIdentifiers = workspace.DataStewardshipDatasetIdentifiers.ToArray()
            };
        }

        filter.ProjectIds = workspace.ProjectIds;
        if (workspace.BirdNestActivityLimit > 0)
        {
            filter.BirdNestActivityLimit = workspace.BirdNestActivityLimit;
        }

        filter.DeterminationFilter = details.IncludeUncertainIdentification
            ? null
            : SightingDeterminationFilterDto.NotUnsureDetermination;

        filter.ExtendedFilter = new ExtendedFilterDto();
        SetPresence(filter, details.Presence);
        SetSpontaneous(filter.ExtendedFilter, details.Spontaneous);
        SetExtendedType(filter.ExtendedFilter, details.ExtendedTypeFilter);
        filter.ExtendedFilter.HasInvasiveSpeciesTreatment =
            details.HasInvasiveSpeciesTreatment ? true : null;

        if (workspace.ActivityIds?.Count > 0)
        {
            filter.ExtendedFilter.ActivityIds = workspace.ActivityIds.ToArray();
        }

        SetDateFilter(filter, dateFilter);
        ApplyDateOverrides(filter, dateFilter, overrides);
        SetGeographics(filter, workspace, details, overrides);

        if (workspace.ExcludedOccurrenceIds?.Count > 0)
        {
            filter.ExcludeFilter = new ExcludeFilterDto
            {
                OccurrenceIds = workspace.ExcludedOccurrenceIds.ToArray()
            };
        }

        if (overrides?.TaxonIds is not null)
        {
            filter.Taxon = new TaxonFilterDto
            {
                Ids = overrides.TaxonIds.Count > 0
                    ? overrides.TaxonIds.ToArray()
                    : NonMatchingTaxonSelection
            };
        }
        else if (workspace.TaxonSelection is not null)
        {
            var taxonIds = await _taxonSelectionResolver.ResolveAsync(
                workspace.TaxonSelection,
                cancellationToken);
            filter.Taxon = new TaxonFilterDto
            {
                Ids = taxonIds.Count > 0
                    ? taxonIds.ToArray()
                    : NonMatchingTaxonSelection
            };
        }
    }

    private static void SetPresence(SearchFilterInternalBaseDto filter, WorkspacePresence presence)
    {
        if (presence == WorkspacePresence.None)
        {
            presence = WorkspacePresence.ObservedOnly;
        }

        switch (presence)
        {
            case WorkspacePresence.ObservedOnly:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.NoFilter;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.DontIncludeNotPresent;
                break;
            case WorkspacePresence.ObservedAndNotRecovered:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.IncludeNotRecovered;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.DontIncludeNotPresent;
                break;
            case WorkspacePresence.ObservedAndNotFound:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.DontIncludeNotRecovered;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.IncludeNotPresent;
                break;
            case WorkspacePresence.NotRecoveredAndNotFoundOnly:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.NoFilter;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.IncludeNotPresent;
                break;
            case WorkspacePresence.OnlyNotRecovered:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.OnlyNotRecovered;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.DontIncludeNotPresent;
                break;
            case WorkspacePresence.OnlyNotFound:
                filter.OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent;
                filter.NotRecoveredFilter = SightingNotRecoveredFilterDto.DontIncludeNotRecovered;
                filter.ExtendedFilter!.NotPresentFilter = SightingNotPresentFilterDto.OnlyNotPresent;
                break;
            default:
                throw Validation(nameof(WorkspaceObservationDetails.Presence), presence);
        }
    }

    private static void SetSpontaneous(ExtendedFilterDto filter, WorkspaceSpontaneous spontaneous)
    {
        if (spontaneous == WorkspaceSpontaneous.None)
        {
            spontaneous = WorkspaceSpontaneous.SpontaneousAndUnspontaneous;
        }

        filter.UnspontaneousFilter = spontaneous switch
        {
            WorkspaceSpontaneous.SpontaneousAndUnspontaneous => SightingUnspontaneousFilterDto.NoFilter,
            WorkspaceSpontaneous.SpontaneousOnly => SightingUnspontaneousFilterDto.NotUnspontaneous,
            WorkspaceSpontaneous.UnspontaneousOnly => SightingUnspontaneousFilterDto.Unspontaneous,
            _ => throw Validation(nameof(WorkspaceObservationDetails.Spontaneous), spontaneous)
        };
    }

    private static void SetExtendedType(
        ExtendedFilterDto filter,
        WorkspaceExtendedTypeFilter extendedTypeFilter)
    {
        filter.SightingTypeSearchGroupIds = extendedTypeFilter switch
        {
            WorkspaceExtendedTypeFilter.DoNotShowMerged => [1, 4, 16, 32, 128],
            WorkspaceExtendedTypeFilter.DoNotShowMergedIncludeReplacementChildren => [1, 8, 16, 64, 256],
            _ => throw Validation(nameof(WorkspaceObservationDetails.ExtendedTypeFilter), extendedTypeFilter)
        };
    }

    private void SetDateFilter(SearchFilterInternalBaseDto filter, WorkspaceDateFilter workspaceDate)
    {
        if (workspaceDate.Mode == WorkspaceDateFilterMode.AllYears)
        {
            return;
        }

        var today = GetTodayInSweden();
        if (workspaceDate.FilterByObservationDate || workspaceDate.IgnoreYear)
        {
            DateTime start;
            DateTime end;
            if (workspaceDate.Mode == WorkspaceDateFilterMode.Custom)
            {
                start = workspaceDate.StartDate?.Date ?? DateTime.MinValue.Date;
                end = workspaceDate.EndDate?.Date ??
                    (workspaceDate.IgnoreYear ? new DateTime(1, 12, 31) : today.AddDays(1));

                if (workspaceDate.IgnoreYear)
                {
                    try
                    {
                        var anchorYear = (start.Month == 2 && start.Day == 29 || end.Month == 2 && end.Day == 29)
                            && !DateTime.IsLeapYear(today.Year) ? 2000 : today.Year;
                        start = new DateTime(anchorYear, start.Month, start.Day);
                        end = new DateTime(anchorYear, end.Month, end.Day);
                    }
                    catch (ArgumentOutOfRangeException exception)
                    {
                        throw new SosWorkspaceFilterValidationException(
                            nameof(WorkspaceDateFilter),
                            "The selected month and day are not valid in the current year.",
                            innerException: exception);
                    }

                    filter.ExtendedFilter!.UsePeriodForAllYears = true;
                    if (workspaceDate.StartYear.HasValue || workspaceDate.EndYear.HasValue)
                    {
                        filter.ExtendedFilter.Years = GenerateYearRange(
                            workspaceDate.StartYear,
                            workspaceDate.EndYear);
                    }
                }
            }
            else
            {
                (start, end) = GetRelativeDateRange(workspaceDate.Mode, today);
            }

            if (!workspaceDate.IgnoreYear && start > end)
                throw new SosWorkspaceFilterValidationException(nameof(WorkspaceDateFilter.StartDate),
                    "StartDate must be less than or equal to EndDate.");

            filter.Date = new DateFilterDto
            {
                StartDate = AtSwedishLocalTime(start),
                EndDate = AtEndOfSwedishDay(end),
                DateFilterType = workspaceDate.IncludePeriodObservations
                    ? null
                    : DateFilterTypeDto.BetweenStartDateAndEndDate
            };
        }
        else
        {
            var (start, end) = workspaceDate.Mode == WorkspaceDateFilterMode.Custom
                ? (workspaceDate.StartDate?.Date ?? DateTime.MinValue.Date,
                    workspaceDate.EndDate?.Date ?? today.AddDays(1))
                : GetRelativeDateRange(workspaceDate.Mode, today);

            if (start > end)
                throw new SosWorkspaceFilterValidationException(nameof(WorkspaceDateFilter.StartDate),
                    "StartDate must be less than or equal to EndDate.");
            filter.ExtendedFilter!.ReportedDateFrom = AtSwedishLocalTime(start);
            filter.ExtendedFilter.ReportedDateTo = AtSwedishLocalTime(end);
        }
    }

    private void ApplyDateOverrides(
        SearchFilterInternalBaseDto filter,
        WorkspaceDateFilter workspaceDate,
        WorkspaceFilterOverrides? overrides)
    {
        if (overrides?.DateFrom is null && overrides?.DateTo is null)
        {
            return;
        }

        var start = overrides.DateFrom?.Date;
        var end = overrides.DateTo?.Date;
        var today = GetTodayInSweden();

        if (start > end)
            throw new SosWorkspaceFilterValidationException(nameof(WorkspaceFilterOverrides.DateFrom),
                "DateFrom must be less than or equal to DateTo.");

        if (workspaceDate.Mode == WorkspaceDateFilterMode.Custom && workspaceDate.IgnoreYear)
        {
            ApplyRecurringDateOverrides(filter, start, end);
            return;
        }

        var savedStart = filter.Date?.StartDate?.Date ?? filter.ExtendedFilter?.ReportedDateFrom?.Date;
        var savedEnd = filter.Date?.EndDate?.Date ?? filter.ExtendedFilter?.ReportedDateTo?.Date;
        start = !start.HasValue || start < savedStart ? savedStart : start;
        end = !end.HasValue || end > savedEnd ? savedEnd : end;
        start ??= DateTime.MinValue.Date;
        end ??= today.AddDays(1);
        if (start > end)
            throw new SosWorkspaceFilterValidationException(nameof(WorkspaceFilterOverrides.DateFrom),
                "The requested date interval does not overlap the workspace date interval.");

        if (workspaceDate.FilterByObservationDate || workspaceDate.IgnoreYear)
        {
            filter.Date = new DateFilterDto
            {
                StartDate = AtSwedishLocalTime(start ?? DateTime.MinValue.Date),
                EndDate = AtEndOfSwedishDay(end ?? today.AddDays(1)),
                DateFilterType = workspaceDate.IncludePeriodObservations
                    ? null
                    : DateFilterTypeDto.BetweenStartDateAndEndDate
            };
        }
        else
        {
            filter.ExtendedFilter ??= new ExtendedFilterDto();
            filter.ExtendedFilter.ReportedDateFrom = AtSwedishLocalTime(start ?? DateTime.MinValue.Date);
            filter.ExtendedFilter.ReportedDateTo = AtSwedishLocalTime(end ?? today.AddDays(1));
        }
    }

    private static void ApplyRecurringDateOverrides(
        SearchFilterInternalBaseDto filter, DateTime? start, DateTime? end)
    {
        // A single SOS interval cannot represent an arbitrary intersection with a recurring period.
        if (!start.HasValue || !end.HasValue || start.Value.Year != end.Value.Year)
            throw new SosWorkspaceFilterValidationException(nameof(WorkspaceFilterOverrides.DateFrom),
                "A recurring period requires both override dates within one calendar year.");

        var year = start.Value.Year;
        var periodStart = filter.Date!.StartDate!.Value.Date;
        var periodEnd = filter.Date.EndDate!.Value.Date;
        var segments = new List<(DateTime Start, DateTime End)>();
        if (filter.ExtendedFilter!.Years is null || filter.ExtendedFilter.Years.Contains(year))
        {
            var first = new DateTime(year, periodStart.Month, Math.Min(periodStart.Day, DateTime.DaysInMonth(year, periodStart.Month)));
            if (periodStart.Day > DateTime.DaysInMonth(year, periodStart.Month)) first = first.AddDays(1);
            var last = new DateTime(year, periodEnd.Month, Math.Min(periodEnd.Day, DateTime.DaysInMonth(year, periodEnd.Month)));
            if (periodStart <= periodEnd) Add(first, last);
            else
            {
                Add(new DateTime(year, 1, 1), last);
                Add(first, new DateTime(year, 12, 31));
            }
        }
        if (segments.Count != 1)
            throw new SosWorkspaceFilterValidationException(nameof(WorkspaceFilterOverrides.DateFrom),
                segments.Count == 0 ? "The requested interval does not overlap the recurring period." :
                "The intersection contains two periods. Request each period separately.");

        filter.Date.StartDate = AtSwedishLocalTime(segments[0].Start);
        filter.Date.EndDate = AtEndOfSwedishDay(segments[0].End);
        filter.ExtendedFilter.UsePeriodForAllYears = false;
        filter.ExtendedFilter.Years = null;

        void Add(DateTime first, DateTime last)
        {
            first = first < start.Value ? start.Value : first;
            last = last > end.Value ? end.Value : last;
            if (first <= last) segments.Add((first, last));
        }
    }

    private void SetGeographics(
        SearchFilterInternalBaseDto filter,
        SosWorkspaceSearchDefinition workspace,
        WorkspaceObservationDetails details,
        WorkspaceFilterOverrides? overrides)
    {
        var geographics = new GeographicsFilterDto
        {
            IncludeObservationsOutsideSweden = !(workspace.ExcludeObservationsOutsideSweden ?? false),
            BoundingBox = overrides?.BoundingBox
        };

        if (workspace.Areas?.Count > 0)
        {
            geographics.Areas = workspace.Areas.Select(area => new AreaFilterDto
            {
                AreaType = ParseAreaType(area.AreaType),
                FeatureId = area.FeatureId
            }).ToArray();
        }

        if (workspace.Geometries?.Count > 0)
        {
            geographics.Geometries = workspace.Geometries.ToList();
        }

        if (overrides?.Point is not null)
        {
            geographics.Geometries ??= new List<IGeometryObject>();
            // GeoJSON.Text's Position constructor takes latitude before longitude,
            // while GeoJSON is serialized as [longitude, latitude].
            geographics.Geometries.Add(new Point(new Position(
                overrides.Point.Latitude,
                overrides.Point.Longitude)));
            geographics.MaxDistanceFromPoint = overrides.RadiusInMeters ?? overrides.DefaultRadiusInMeters;
        }

        if (details.FilterOnMaxAccuracy)
        {
            geographics.MaxAccuracy = details.MaxAccuracy;
        }

        filter.Geographics = geographics;
    }

    private int[] GenerateYearRange(int? startYear, int? endYear)
    {
        var start = startYear ?? 1700;
        var end = endYear ?? GetTodayInSweden().Year;
        if (start > end)
        {
            throw new SosWorkspaceFilterValidationException(
                nameof(WorkspaceDateFilter.StartYear),
                "StartYear must be less than or equal to EndYear.");
        }

        return Enumerable.Range(start, end - start + 1).ToArray();
    }

    private DateTime GetTodayInSweden() =>
        TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), SwedishTime.TimeZone).Date;

    private static DateTimeOffset AtSwedishLocalTime(DateTime value) => SwedishTime.FromLocalDateTime(value);

    private static DateTimeOffset AtEndOfSwedishDay(DateTime value) =>
        AtSwedishLocalTime(value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));


    private static (DateTime Start, DateTime End) GetRelativeDateRange(
        WorkspaceDateFilterMode mode,
        DateTime today)
    {
        var startYear = mode switch
        {
            WorkspaceDateFilterMode.CurrentYear => today.Year,
            WorkspaceDateFilterMode.PreviousYear => today.Year - 1,
            WorkspaceDateFilterMode.LastFiveCalendarYears => today.Year - 4,
            WorkspaceDateFilterMode.LastTwentyFiveCalendarYears => today.Year - 24,
            WorkspaceDateFilterMode.LastFiveYears => today.Year - 5,
            WorkspaceDateFilterMode.LastTwentyFiveYears => today.Year - 25,
            _ => throw Validation(nameof(WorkspaceDateFilter.Mode), mode)
        };
        var endYear = mode == WorkspaceDateFilterMode.PreviousYear ? today.Year - 1 : today.Year;
        return (new DateTime(startYear, 1, 1), new DateTime(endYear, 12, 31));
    }

    private static AreaTypeDto ParseAreaType(string value)
    {
        if (Enum.TryParse<AreaTypeDto>(value, true, out var areaType))
        {
            return areaType;
        }

        throw new SosWorkspaceFilterValidationException(
            nameof(WorkspaceAreaSelection.AreaType),
            $"Area type '{value}' is not supported.",
            Enum.GetNames<AreaTypeDto>());
    }

    private static SosWorkspaceFilterValidationException Validation<TEnum>(
        string parameterName,
        TEnum value)
        where TEnum : struct, Enum
    {
        return new SosWorkspaceFilterValidationException(
            parameterName,
            $"Value '{value}' is not supported for {parameterName}.",
            Enum.GetNames<TEnum>());
    }
}
