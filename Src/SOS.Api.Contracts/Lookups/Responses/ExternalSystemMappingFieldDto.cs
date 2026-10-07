namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>One mapped field in an external system.</summary>
public class ExternalSystemMappingFieldDto
{

    /// <summary>
    /// The key in the external system.
    /// </summary>
    public string Key { get; set; }

    /// <summary>A description of the field in the external system.</summary>
    public string Description { get; set; }

    /// <summary>The external field values and their corresponding SOS vocabulary identifiers.</summary>
    public ICollection<ExternalSystemMappingValueDto> Values { get; set; }

}


