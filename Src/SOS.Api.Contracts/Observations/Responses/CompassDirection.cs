using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// States the wind direction during the survey event as a compass direction.
/// </summary>
public enum CompassDirection
{
    /// <summary>Nord.</summary>
    [EnumMember(Value = "nord")]
    North = 0,

    /// <summary>Nordost.</summary>
    [EnumMember(Value = "nordost")]
    Northeast = 1,

    /// <summary>Nordväst.</summary>
    [EnumMember(Value = "nordväst")]
    Northwest = 2,

    /// <summary>Ost.</summary>
    [EnumMember(Value = "ost")]
    East = 3,

    /// <summary>Syd.</summary>
    [EnumMember(Value = "syd")]
    South = 4,

    /// <summary>Sydost.</summary>
    [EnumMember(Value = "sydost")]
    Southeast = 5,

    /// <summary>Sydväst.</summary>
    [EnumMember(Value = "sydväst")]
    Southwest = 6,

    /// <summary>Väst.</summary>
    [EnumMember(Value = "väst")]
    West = 7,
}


