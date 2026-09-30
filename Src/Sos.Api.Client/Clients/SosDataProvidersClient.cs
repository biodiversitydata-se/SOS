using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Lookups.Responses;
using System.Text.Json.Nodes;
using System.Xml;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosDataProvidersClient : ISosDataProvidersClient
{
    private readonly HttpClient _httpClient;

    private static void ThrowIfMissingDataProviderId(int dataProviderId)
    {
        if (dataProviderId <= 0)
        {
            throw new ArgumentException("Data provider id must be greater than zero.", nameof(dataProviderId));
        }
    }

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

    /// <inheritdoc />
    public async Task<XmlDocument?> GetDataProviderEmlAsXmlAsync(
        int dataProviderId,
        DataProviderEmlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfMissingDataProviderId(dataProviderId);

        options ??= new DataProviderEmlOptions();

        using var request = new HttpRequestMessage(
             HttpMethod.Get,
             $"dataproviders/{dataProviderId}/EML?{options.ToQueryString()}");
        request.ApplySosRequestHeaders(acceptProfile: SosAcceptProfile.Text);
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en-GB"));

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
        {
            var xml = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<JsonObject?> GetDataProviderEmlAsJsonAsync(
        int dataProviderId,
        DataProviderEmlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfMissingDataProviderId(dataProviderId);

        options ??= new DataProviderEmlOptions();

        using var request = new HttpRequestMessage(
             HttpMethod.Get,
             $"dataproviders/{dataProviderId}/EML?{options.ToQueryString()}");
        request.ApplySosRequestHeaders(acceptProfile: SosAcceptProfile.Json);
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en-GB"));

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var document = JsonNode.Parse(json) as JsonObject;
            return document;
        }

        return null;
    }
}
