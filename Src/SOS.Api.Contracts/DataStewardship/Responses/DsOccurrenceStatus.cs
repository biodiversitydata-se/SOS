using System.Runtime.Serialization;

namespace SOS.Api.Contracts.DataStewardship.Responses;

/// <summary>
/// States whether a specific taxon was observed or not. Observations with \"Förekomst\" = \"inte observerad\" are so called zero observations.
/// </summary>
public enum DsOccurrenceStatus
{
    /// <summary>
    /// inte observerad
    /// </summary>
    [EnumMember(Value = "inte observerad")]
    InteObserverad = 0,
    /// <summary>
    /// observerad
    /// </summary>
    [EnumMember(Value = "observerad")]
    Observerad = 1
}
