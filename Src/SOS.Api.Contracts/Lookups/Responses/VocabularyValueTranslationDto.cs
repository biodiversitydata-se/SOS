namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>A vocabulary value translated into one culture.</summary>
public class VocabularyValueTranslationDto
{

    /// <summary>
    /// Culture code. I.e. en-GB, sv-SE
    /// </summary>
    public string CultureCode { get; set; }

    /// <summary>
    /// Translation
    /// </summary>
    public string Value { get; set; }

}


