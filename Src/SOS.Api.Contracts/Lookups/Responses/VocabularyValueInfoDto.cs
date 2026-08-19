namespace SOS.Api.Contracts.Lookups.Responses;
public class VocabularyValueInfoDto
{

    public int Id { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    public VocabularyValueInfoCategoryDto Category { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


