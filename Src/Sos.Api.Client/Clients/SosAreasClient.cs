using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosAreasClient : ISosAreasClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates an area client using the supplied HTTP client.</summary>
    public SosAreasClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<AreaBaseDtoPagedResult?> GetAreasAsync(
        AreasOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestOptions = options ?? new AreasOptions();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"areas?{requestOptions.ToQueryString()}");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<AreaBaseDtoPagedResult>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AreaBaseDto?> GetAreaAsync(
        AreaTypeDto areaType,
        string featureId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            throw new ArgumentException("Feature id must not be empty.", nameof(featureId));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"areas/{Uri.EscapeDataString(areaType.ToString())}/{Uri.EscapeDataString(featureId)}");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<AreaBaseDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAreaExportAsync(
        AreaTypeDto areaType,
        string featureId,
        AreaExportFormatDto format = AreaExportFormatDto.Json,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            throw new ArgumentException("Feature id must not be empty.", nameof(featureId));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"areas/{Uri.EscapeDataString(areaType.ToString())}/{Uri.EscapeDataString(featureId)}/export?format={Uri.EscapeDataString(format.ToString())}");
        request.ApplySosRequestHeaders(acceptProfile: SosAcceptProfile.Any);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw SosClientHttp.CreateApiException(
                response,
                await SosClientHttp.ReadErrorResponsePreviewAsync(response, cancellationToken),
                request.RequestUri);
        }

        var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return response.StatusCode == System.Net.HttpStatusCode.NoContent ? null : responseBody;
    }
}
