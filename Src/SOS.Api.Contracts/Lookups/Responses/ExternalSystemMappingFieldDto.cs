namespace SOS.Api.Contracts.Lookups.Responses;
public class ExternalSystemMappingFieldDto
{

    /// <summary>
    /// The key in the external system.
    /// </summary>
    public string Key { get; set; }

    public string Description { get; set; }

    public ICollection<ExternalSystemMappingValueDto> Values { get; set; }

}


