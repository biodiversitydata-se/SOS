using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations;
/// <summary>
/// Observation diffusion status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DiffusionStatus>))]
public enum DiffusionStatus
{
    /// <summary>Observation is not diffused.</summary>
    NotDiffused = 0,
    /// <summary>
    /// Observation is diffused by the system; the original exists in the protected index.
    /// </summary>
    DiffusedBySystem = 1,
    /// <summary>Observation is diffused by the provider; no original exists in the system.</summary>
    DiffusedByProvider = 2,
}


