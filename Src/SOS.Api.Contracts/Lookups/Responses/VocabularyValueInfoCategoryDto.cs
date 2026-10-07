namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Category information for a vocabulary value.</summary>
public class VocabularyValueInfoCategoryDto
{

    /// <summary>The category identifier.</summary>
    public int Id { get; set; }

    /// <summary>The category name.</summary>
    public string Name { get; set; }

    /// <summary>A description of the category.</summary>
    public string Description { get; set; }

    /// <summary>Whether the category has translations.</summary>
    public bool Localized { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


