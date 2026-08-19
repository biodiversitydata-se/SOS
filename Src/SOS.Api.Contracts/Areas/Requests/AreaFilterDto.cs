using SOS.Api.Contracts.Areas.Responses;

namespace SOS.Api.Contracts.Areas.Requests;
/// <summary>
/// Area filter.
/// </summary>
public class AreaFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    /// Feature
    /// </summary>
    public string FeatureId { get; set; }

}


