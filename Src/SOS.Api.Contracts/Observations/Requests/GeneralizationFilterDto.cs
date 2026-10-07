namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Generalization filter.
/// </summary>
public class GeneralizationFilterDto
{

    /// <summary>How sensitive observations should be generalized.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SensitiveGeneralizationFilterDto? SensitiveGeneralizationFilter { get; set; }

    /// <summary>How public observations should be generalized.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public PublicGeneralizationFilterDto? PublicGeneralizationFilter { get; set; }

}


