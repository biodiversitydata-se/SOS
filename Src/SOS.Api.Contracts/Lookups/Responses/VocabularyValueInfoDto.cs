namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Detailed information about a vocabulary value.</summary>
public class VocabularyValueInfoDto
{

    /// <summary>The vocabulary value identifier.</summary>
    public int Id { get; set; }

    /// <summary>The display value.</summary>
    public string Value { get; set; }

    /// <summary>A description of the vocabulary value.</summary>
    public string Description { get; set; }

    /// <summary>Whether the value has translations.</summary>
    public bool Localized { get; set; }

    /// <summary>The category to which the value belongs.</summary>
    public VocabularyValueInfoCategoryDto Category { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


