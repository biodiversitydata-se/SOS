using System.Runtime.Serialization;

namespace SOS.Api.Contracts.Observations;
/// <summary>
///     The reason for the sensitivity of the observation.
/// </summary>
public enum SensitivityReason
{
    /// <summary>
    /// Reason for sensitivity is unknown.
    /// </summary>
    [EnumMember(Value = "Unknown")]
    Unknown = 0,
    /// <summary>
    /// Sensitive species.
    /// </summary>
    [EnumMember(Value = "SensitiveSpecies")]
    SensitiveSpecies = 1,
    /// <summary>
    /// Provider decision
    /// </summary>
    [EnumMember(Value = "ProviderDecision")]
    ProviderDecision = 2,
    /// <summary>
    /// Reporter decision
    /// </summary>
    [EnumMember(Value = "ReporterDecision")]
    ReporterDecision = 3,
    /// <summary>
    /// Location generalization
    /// </summary>
    [EnumMember(Value = "LocationGeneralization")]
    LocationGeneralization = 4
}


