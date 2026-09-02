namespace SOS.Api.Contracts.Observations.Responses;
public class Measuring
{

    /// <summary>
    /// Value for measured weather variable.
    /// </summary>
    public double? Value { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Unit Unit { get; set; }

}


