using System.Runtime.Serialization;

namespace SOS.Lib.Enums;

/// <summary>
///     The reason for the sensitivity of the observation.
/// </summary>
public enum SensitivityReason
{
    [EnumMember(Value = "Unknown")]
    Unknown = 0,
    [EnumMember(Value = "SensitiveSpecies")]
    SensitiveSpecies = 1,
    [EnumMember(Value = "ProviderDecision")]
    ProviderDecision = 2,
    [EnumMember(Value = "ReporterDecision")]
    ReporterDecision = 3,
    [EnumMember(Value = "LocationGeneralization")]
    LocationGeneralization = 4
}