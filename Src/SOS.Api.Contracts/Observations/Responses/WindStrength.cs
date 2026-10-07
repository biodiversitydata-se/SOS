using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// States the strength of the wind during the survey event.
/// </summary>
public enum WindStrength
{
    /// <summary>0 Beaufort.</summary>
    [EnumMember(Value = "0 Beaufort")]
    _0Beaufort = 0,

    /// <summary>1 Beaufort.</summary>
    [EnumMember(Value = "1 Beaufort")]
    _1Beaufort = 1,

    /// <summary>2 Beaufort.</summary>
    [EnumMember(Value = "2 Beaufort")]
    _2Beaufort = 2,

    /// <summary>3 Beaufort.</summary>
    [EnumMember(Value = "3 Beaufort")]
    _3Beaufort = 3,

    /// <summary>4 Beaufort.</summary>
    [EnumMember(Value = "4 Beaufort")]
    _4Beaufort = 4,

    /// <summary>5 Beaufort.</summary>
    [EnumMember(Value = "5 Beaufort")]
    _5Beaufort = 5,

    /// <summary>6 Beaufort.</summary>
    [EnumMember(Value = "6 Beaufort")]
    _6Beaufort = 6,

    /// <summary>7 Beaufort.</summary>
    [EnumMember(Value = "7 Beaufort")]
    _7Beaufort = 7,

    /// <summary>8 Beaufort.</summary>
    [EnumMember(Value = "8 Beaufort")]
    _8Beaufort = 8,

    /// <summary>9 Beaufort.</summary>
    [EnumMember(Value = "9 Beaufort")]
    _9Beaufort = 9,

    /// <summary>10 Beaufort.</summary>
    [EnumMember(Value = "10 Beaufort")]
    _10Beaufort = 10,

    /// <summary>11 Beaufort.</summary>
    [EnumMember(Value = "11 Beaufort")]
    _11Beaufort = 11,

    /// <summary>12 Beaufort.</summary>
    [EnumMember(Value = "12 Beaufort")]
    _12Beaufort = 12,

    /// <summary>Vindstilla, under 1 m/s.</summary>
    [EnumMember(Value = "vindstilla, <1 m/s")]
    Calm1Ms = 13,

    /// <summary>Svag vind, 1-3 m/s.</summary>
    [EnumMember(Value = "svag vind, 1-3 m/s")]
    LightBreezeUpTo3Ms = 14,

    /// <summary>Måttlig vind, 4-7 m/s.</summary>
    [EnumMember(Value = "måttlig vind, 4-7 m/s")]
    ModerateBreeze4To7Ms = 15,

    /// <summary>Frisk vind, 8-13 m/s.</summary>
    [EnumMember(Value = "frisk vind, 8-13 m/s")]
    FreshBreeze8Till13Ms = 16,

    /// <summary>Hård vind, 14-19 m/s.</summary>
    [EnumMember(Value = "hård vind, 14-19 m/s")]
    NearGale14To19Ms = 17,

    /// <summary>Mycket hård vind, 20-24 m/s.</summary>
    [EnumMember(Value = "mycket hård vind, 20-24 m/s")]
    StrongGale20To24Ms = 18,

    /// <summary>Storm, 25-32 m/s.</summary>
    [EnumMember(Value = "storm, 25-32 m/s")]
    Storm25Till32Ms = 19,

    /// <summary>Orkan, minst 33 m/s.</summary>
    [EnumMember(Value = "orkan, ≥33 m/s")]
    Hurricane33Ms = 20,
}


