using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Export property sets.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<OutputFieldSet>))]
public enum OutputFieldSet
{
    /// <summary>Minimum of properties exported.</summary>
    Minimum = 0,
    /// <summary>An extended set of properties exported.</summary>
    Extended = 1,
    /// <summary>All properties where at least one observation has a value.</summary>
    AllWithValues = 2,
    /// <summary>All properties exported.</summary>
    All = 3,
    /// <summary>The field does not belong to any field set.</summary>
    None = 4,
}


