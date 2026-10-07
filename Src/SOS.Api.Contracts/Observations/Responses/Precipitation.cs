using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// States the precipitation conditions during the survey event.
/// </summary>
public enum Precipitation
{
    /// <summary>Uppehåll.</summary>
    [EnumMember(Value = "uppehåll")]
    DryWeather = 0,

    /// <summary>Lätt regn.</summary>
    [EnumMember(Value = "lätt regn")]
    LightRain = 1,

    /// <summary>Måttligt regn.</summary>
    [EnumMember(Value = "måttligt regn")]
    ModerateRain = 2,

    /// <summary>Kraftigt regn.</summary>
    [EnumMember(Value = "kraftigt regn")]
    HeavyRain = 3,

    /// <summary>Regnskurar.</summary>
    [EnumMember(Value = "regnskurar")]
    Showers = 4,

    /// <summary>Lätt snöfall.</summary>
    [EnumMember(Value = "lätt snöfall")]
    LightSnowfall = 5,

    /// <summary>Måttligt snöfall.</summary>
    [EnumMember(Value = "måttligt snöfall")]
    ModerateSnowfall = 6,

    /// <summary>Kraftigt snöfall.</summary>
    [EnumMember(Value = "kraftigt snöfall")]
    HeavySnowfall = 7,

    /// <summary>Snöbyar.</summary>
    [EnumMember(Value = "snöbyar")]
    Snowflurries = 8,

    /// <summary>Hagelbyar.</summary>
    [EnumMember(Value = "hagelbyar")]
    HailShowers = 9,
}


