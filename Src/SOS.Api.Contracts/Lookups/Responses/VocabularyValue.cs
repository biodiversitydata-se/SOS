namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>
/// Describes a value associated with a vocabulary.
/// </summary>
public class VocabularyValue
{

    /// <summary>
    /// If the entry exist in the vocabulary, then Id is greater than or equal to 0.
    /// If the entry doesn't exist in the vocabulary, then Id is equal to -1.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public string? Value { get; set; }

}


