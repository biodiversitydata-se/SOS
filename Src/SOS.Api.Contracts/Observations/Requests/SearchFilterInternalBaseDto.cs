using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Base filter for internal SOS observation endpoints.
/// </summary>
public class SearchFilterInternalBaseDto : SearchFilterBaseDto
{
    public ExtendedFilterDto? ExtendedFilter { get; set; }
    public GeneralizationFilterDto? GeneralizationFilter { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProtectionFilterDto? ProtectionFilter { get; set; }
}
