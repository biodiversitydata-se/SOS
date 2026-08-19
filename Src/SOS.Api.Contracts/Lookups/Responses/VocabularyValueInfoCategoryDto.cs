namespace SOS.Api.Contracts.Lookups.Responses;
public class VocabularyValueInfoCategoryDto
{

    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


