using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Observations.Responses;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Common filters supported by SOS observation search endpoints.
/// </summary>
public class SearchFilterBaseDto
{
    /// <summary>
    /// Filters that only affect observations from Artportalen.
    /// </summary>
    public ArtportalenFilterDto? Artportalen { get; set; }

    /// <summary>Maximum bird-nest activity level for Artportalen bird observations; other organism groups are unaffected.</summary>
    public int? BirdNestActivityLimit { get; set; }

    /// <summary>Limits observations to selected data providers.</summary>
    public DataProviderFilterDto? DataProvider { get; set; }

    /// <summary>Limits observations to selected data-stewardship datasets.</summary>
    public DataStewardshipFilterDto? DataStewardship { get; set; }

    /// <summary>Filters observations by date.</summary>
    public DateFilterDto? Date { get; set; }

    /// <summary>Filters observations by determination certainty.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SightingDeterminationFilterDto? DeterminationFilter { get; set; }

    /// <summary>Diffusion statuses to include.</summary>
    public ICollection<DiffusionStatus>? DiffusionStatuses { get; set; }

    /// <summary>Filters survey events.</summary>
    public EventFilterDto? Event { get; set; }

    /// <summary>Criteria that observations must match.</summary>
    public IncludeFilterDto? IncludeFilter { get; set; }

    /// <summary>Criteria that observations must not match.</summary>
    public ExcludeFilterDto? ExcludeFilter { get; set; }

    /// <summary>Filters observations by location or geometry.</summary>
    public GeographicsFilterDto? Geographics { get; set; }

    /// <summary>Filters observations by their modification date.</summary>
    public ModifiedDateFilterDto? ModifiedDate { get; set; }

    /// <summary>Filters observations by whether they were not recovered.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto? NotRecoveredFilter { get; set; }

    /// <summary>Only includes observations made by the current user when true.</summary>
    public bool? ObservedByMe { get; set; }

    /// <summary>Selects present and/or absent observations; the default includes present observations only.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

    /// <summary>Project identifiers to match.</summary>
    public ICollection<int>? ProjectIds { get; set; }

    /// <summary>Only includes observations reported by the current user when true.</summary>
    public bool? ReportedByMe { get; set; }

    /// <summary>Filters observations by taxon.</summary>
    public TaxonFilterDto? Taxon { get; set; }

    /// <summary>Requested verification status.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StatusVerificationDto? VerificationStatus { get; set; }
}
