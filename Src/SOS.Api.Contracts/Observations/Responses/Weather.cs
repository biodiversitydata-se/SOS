namespace SOS.Api.Contracts.Observations.Responses;
public class Weather
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SnowCover SnowCover { get; set; }

    public Measuring? Sunshine { get; set; }

    public Measuring? AirTemperature { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public CompassDirection WindDirection { get; set; }

    public Measuring? WindDirectionDegrees { get; set; }

    public Measuring? WindSpeed { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public WindStrength WindStrength { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Precipitation Precipitation { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Visibility Visibility { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Cloudiness Cloudiness { get; set; }

}


