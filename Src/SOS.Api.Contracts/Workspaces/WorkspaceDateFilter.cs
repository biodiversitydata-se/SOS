using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Date-selection settings used to create an SOS observation filter.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceDateFilter
{
    /// <summary>
    /// Controls whether dates apply to observation time. When false, report time is used unless
    /// <see cref="IgnoreYear"/> is enabled, which always uses observation time.
    /// </summary>
    [JsonPropertyName("filterByObservationDate")]
    public bool FilterByObservationDate { get; set; } = true;
    /// <summary>The date-range mode.</summary>
    [JsonPropertyName("mode")]
    public WorkspaceDateFilterMode Mode { get; set; }
    /// <summary>The first date in a <see cref="WorkspaceDateFilterMode.Custom"/> interval.</summary>
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }
    /// <summary>The last date in a <see cref="WorkspaceDateFilterMode.Custom"/> interval.</summary>
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
    /// <summary>
    /// Controls date-interval matching. When true, an observation's start or end date must be within the
    /// interval; when false, both dates must be within it.
    /// </summary>
    [JsonPropertyName("includePeriodObservations")]
    public bool IncludePeriodObservations { get; set; }
    /// <summary>
    /// Controls whether a custom month-and-day interval is applied across every selected year. This mode
    /// always filters on observation time.
    /// </summary>
    [JsonPropertyName("ignoreYear")]
    public bool IgnoreYear { get; set; }
    /// <summary>The optional lower year boundary used with <see cref="IgnoreYear"/>; the default is 1700.</summary>
    [JsonPropertyName("startYear")]
    public int? StartYear { get; set; }
    /// <summary>
    /// The optional upper year boundary used with <see cref="IgnoreYear"/>; the default is the current
    /// Swedish calendar year.
    /// </summary>
    [JsonPropertyName("endYear")]
    public int? EndYear { get; set; }
}
