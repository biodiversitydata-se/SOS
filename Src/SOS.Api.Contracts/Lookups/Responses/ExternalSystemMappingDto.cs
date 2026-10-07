namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Mapping between an external system and SOS vocabulary values.</summary>
public class ExternalSystemMappingDto
{

    /// <summary>The external system or format used by this mapping.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ExternalSystemIdDto Id { get; set; }

    /// <summary>The name of the external system.</summary>
    public string Name { get; set; }

    /// <summary>A description of the external system's vocabulary mapping.</summary>
    public string Description { get; set; }

    /// <summary>The external fields and their mappings to SOS vocabulary values.</summary>
    public ICollection<ExternalSystemMappingFieldDto> Mappings { get; set; }

}


