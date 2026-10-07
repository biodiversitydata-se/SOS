namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>Weather conditions recorded during the survey event.</summary>
public class Weather
{

    /// <summary>Snow conditions on the ground.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SnowCover? SnowCover { get; set; }

    /// <summary>Measured sunshine.</summary>
    public Measuring? Sunshine { get; set; }

    /// <summary>Measured air temperature.</summary>
    public Measuring? AirTemperature { get; set; }

    /// <summary>Wind direction as a compass direction.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public CompassDirection? WindDirection { get; set; }

    /// <summary>Wind direction in degrees.</summary>
    public Measuring? WindDirectionDegrees { get; set; }

    /// <summary>Measured wind speed.</summary>
    public Measuring? WindSpeed { get; set; }

    /// <summary>Wind strength category.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public WindStrength? WindStrength { get; set; }

    /// <summary>Precipitation conditions.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Precipitation? Precipitation { get; set; }

    /// <summary>Visibility conditions.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Visibility? Visibility { get; set; }

    /// <summary>Cloud conditions.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Cloudiness? Cloudiness { get; set; }

}


