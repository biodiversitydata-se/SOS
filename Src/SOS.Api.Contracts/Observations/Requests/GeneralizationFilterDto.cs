namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Generalization filter.
/// </summary>
public class GeneralizationFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SensitiveGeneralizationFilterDto SensitiveGeneralizationFilter { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public PublicGeneralizationFilterDto PublicGeneralizationFilter { get; set; }

}


