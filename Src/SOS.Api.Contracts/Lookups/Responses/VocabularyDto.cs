namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>A controlled vocabulary and its values.</summary>
public class VocabularyDto
{

    /// <summary>The numeric vocabulary identifier.</summary>
    public int Id { get; set; }

    /// <summary>The named vocabulary identifier corresponding to Id.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public VocabularyIdDto EnumId { get; set; }

    /// <summary>The vocabulary name.</summary>
    public string Name { get; set; }

    /// <summary>A description of the vocabulary's purpose.</summary>
    public string Description { get; set; }

    /// <summary>Whether vocabulary values have translations.</summary>
    public bool Localized { get; set; }

    /// <summary>The values defined by this vocabulary.</summary>
    public ICollection<VocabularyValueInfoDto> Values { get; set; }

    /// <summary>Mappings from external systems and formats to this vocabulary's values.</summary>
    public ICollection<ExternalSystemMappingDto> ExternalSystemsMapping { get; set; }

}


