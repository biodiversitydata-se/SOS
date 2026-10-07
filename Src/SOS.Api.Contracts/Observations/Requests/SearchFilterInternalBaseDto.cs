using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Base filter for internal SOS observation endpoints.
/// </summary>
public class SearchFilterInternalBaseDto : SearchFilterBaseDto
{
    /// <summary>Additional filters available to internal callers.</summary>
    public ExtendedFilterDto? ExtendedFilter { get; set; }

    /// <summary>Controls generalization of sensitive and public observations.</summary>
    public GeneralizationFilterDto? GeneralizationFilter { get; set; }

    /// <summary>Controls whether protected observations are included.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProtectionFilterDto? ProtectionFilter { get; set; }
}
