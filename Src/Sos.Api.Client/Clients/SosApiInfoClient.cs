using SOS.Api.Client.Abstractions;
using SOS.Api.Contracts.Metadata.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosApiInfoClient : ISosApiInfoClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates an API-information client using the supplied HTTP client.</summary>
    public SosApiInfoClient(HttpClient httpClient) =>
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <inheritdoc />
    public async Task<ApiInformationDto?> GetApiInfoAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/ApiInfo");
        request.ApplySosRequestHeaders();
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<ApiInformationDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }
}
