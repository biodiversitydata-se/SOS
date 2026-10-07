using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Unit for a certain amount of organisms (given in the attribute quantity) or
/// <br/>Unit for a reported measurement (given in the attribute "vädermått")..
/// </summary>
public enum Unit
{
    /// <summary>Procent.</summary>
    [EnumMember(Value = "%")]
    Percent = 0,

    /// <summary>Kvadratcentimeter.</summary>
    [EnumMember(Value = "cm²")]
    Cm2 = 1,

    /// <summary>Kubikcentimeter.</summary>
    [EnumMember(Value = "cm³")]
    Cm3 = 2,

    /// <summary>Kvadratdecimeter.</summary>
    [EnumMember(Value = "dm²")]
    Dm2 = 3,

    /// <summary>Kompassgrader.</summary>
    [EnumMember(Value = "kompassgrader")]
    Kompassgrader = 4,

    /// <summary>Meter per sekund.</summary>
    [EnumMember(Value = "m/s")]
    Ms = 5,

    /// <summary>Kvadratmeter.</summary>
    [EnumMember(Value = "m²")]
    M2 = 6,

    /// <summary>Stycken.</summary>
    [EnumMember(Value = "styck")]
    Styck = 7,

    /// <summary>Grader Celsius.</summary>
    [EnumMember(Value = "°C")]
    GraderCelsius = 8,
}


