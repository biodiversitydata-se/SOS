namespace SOS.Api.Contracts.Lookups.Responses;
public class ExternalSystemMappingDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ExternalSystemIdDto Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ICollection<ExternalSystemMappingFieldDto> Mappings { get; set; }

}


