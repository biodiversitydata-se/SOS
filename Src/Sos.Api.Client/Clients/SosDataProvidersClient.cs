using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosDataProvidersClient : ISosDataProvidersClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a data-provider client using the supplied HTTP client.</summary>
    public SosDataProvidersClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DataProviderDto>> GetDataProvidersAsync(
        DataProviderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestOptions = options ?? new DataProviderOptions();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"dataproviders?{requestOptions.ToQueryString()}");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var dataProviders = await SosClientHttp.ReadJsonResponseAsync<List<DataProviderDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return dataProviders ?? [];
    }
}
