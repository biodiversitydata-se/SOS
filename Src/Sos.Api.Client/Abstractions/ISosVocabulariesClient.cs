using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS vocabulary lookup endpoints.</summary>
public interface ISosVocabulariesClient
{
    /// <summary>Gets all available vocabularies.</summary>
    Task<IReadOnlyList<VocabularyDto>> GetVocabulariesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Gets one vocabulary by identifier.</summary>
    Task<VocabularyDto?> GetVocabularyByIdAsync(
        VocabularyIdDto vocabularyId,
        CancellationToken cancellationToken = default);
}
