namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>A measured weather value and its unit.</summary>
public class Measuring
{

    /// <summary>
    /// Value for measured weather variable.
    /// </summary>
    public double? Value { get; set; }

    /// <summary>Unit of the measured value.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Unit? Unit { get; set; }

}


