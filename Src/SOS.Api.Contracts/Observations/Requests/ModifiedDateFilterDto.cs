namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Filter for observation Modified date
/// </summary>
public class ModifiedDateFilterDto
{

    /// <summary>
    /// Changed from
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? From { get; set; }

    /// <summary>
    /// Changed tp
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? To { get; set; }

}


