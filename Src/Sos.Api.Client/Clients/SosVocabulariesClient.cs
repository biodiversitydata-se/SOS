using SOS.Api.Client.Abstractions;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosVocabulariesClient : ISosVocabulariesClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a vocabulary client using the supplied HTTP client.</summary>
    public SosVocabulariesClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VocabularyDto>> GetVocabulariesAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "vocabularies");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var vocabularies = await SosClientHttp.ReadJsonResponseAsync<List<VocabularyDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return vocabularies ?? [];
    }

    /// <inheritdoc />
    public async Task<VocabularyDto?> GetVocabularyByIdAsync(
        VocabularyIdDto vocabularyId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"vocabularies/{Uri.EscapeDataString(vocabularyId.ToString())}");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<VocabularyDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }
}
