using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// States the visibility conditions during the survey event.
/// </summary>
public enum Visibility
{
    /// <summary>Dimma, sikt kortare än 1 km.</summary>
    [EnumMember(Value = "dimma, <1 km")]
    Fog1Km = 0,

    /// <summary>Dis, sikt 1-4 km.</summary>
    [EnumMember(Value = "dis, 1-4 km")]
    Haze1To4Km = 1,

    /// <summary>Måttlig sikt, 4-10 km.</summary>
    [EnumMember(Value = "måttlig, 4-10 km")]
    Moderate4To10Km = 2,

    /// <summary>God sikt, 10-20 km.</summary>
    [EnumMember(Value = "god, 10-20 km")]
    Good10To20Km = 3,

    /// <summary>Mycket god sikt, över 20 km.</summary>
    [EnumMember(Value = "mycket god, >20 km")]
    VeryGood20Km = 4,
}


