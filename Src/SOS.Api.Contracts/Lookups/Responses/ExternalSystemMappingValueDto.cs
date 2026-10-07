using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Correspondence between an external value and an SOS vocabulary value.</summary>
public class ExternalSystemMappingValueDto
{

    /// <summary>
    /// Value in data provider.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Id in SOS (Species Observation System).
    /// </summary>
    public int SosId { get; set; }

}


