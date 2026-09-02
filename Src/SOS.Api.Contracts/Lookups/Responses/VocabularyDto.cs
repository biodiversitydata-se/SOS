namespace SOS.Api.Contracts.Lookups.Responses;
public class VocabularyDto
{

    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public VocabularyIdDto EnumId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    public ICollection<VocabularyValueInfoDto> Values { get; set; }

    public ICollection<ExternalSystemMappingDto> ExternalSystemsMapping { get; set; }

}


