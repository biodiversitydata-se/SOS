using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Lookups.Responses;
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


