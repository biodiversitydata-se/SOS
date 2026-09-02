using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Observations.Responses;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Common filters supported by SOS observation search endpoints.
/// </summary>
public class SearchFilterBaseDto
{
    public int? BirdNestActivityLimit { get; set; }
    public DataProviderFilterDto? DataProvider { get; set; }
    public DataStewardshipFilterDto? DataStewardship { get; set; }
    public DateFilterDto? Date { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SightingDeterminationFilterDto? DeterminationFilter { get; set; }

    public ICollection<DiffusionStatusDto>? DiffusionStatuses { get; set; }
    public EventFilterDto? Event { get; set; }
    public IncludeFilterDto? IncludeFilter { get; set; }
    public ExcludeFilterDto? ExcludeFilter { get; set; }
    public GeographicsFilterDto? Geographics { get; set; }
    public ModifiedDateFilterDto? ModifiedDate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto? NotRecoveredFilter { get; set; }

    public bool? ObservedByMe { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

    public ICollection<int>? ProjectIds { get; set; }
    public bool? ReportedByMe { get; set; }
    public TaxonFilterDto? Taxon { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StatusVerificationDto? VerificationStatus { get; set; }
}
