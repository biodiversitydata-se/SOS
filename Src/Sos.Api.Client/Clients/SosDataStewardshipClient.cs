using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosDataStewardshipClient : ISosDataStewardshipClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a data-stewardship client using the supplied HTTP client.</summary>
    public SosDataStewardshipClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<DsDatasetDto?> GetDatasetAsync(
        string id,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Dataset id must not be empty.", nameof(id));
        }

        var requestOptions = options ?? new SosRoleRequestOptions();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"DataStewardship/datasets/{Uri.EscapeDataString(id)}");
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<DsDatasetDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<DsDatasetDto>?> GetDatasetsByFilterAsync(
        string filter,
        DataStewardshipDatasetOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestOptions = options ?? new DataStewardshipDatasetOptions();
        var query = new SosQueryBuilder()
            .Add("filter", filter ?? string.Empty)
            .Add("skip", requestOptions.Skip)
            .Add("take", requestOptions.Take);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            query.AppendTo("DataStewardship/datasets"));
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<PagedResultDto<DsDatasetDto>>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<DsDatasetDto>?> SearchDatasetsAsync(
        SearchFilterBaseDto filter,
        DataStewardshipDatasetOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var requestOptions = options ?? new DataStewardshipDatasetOptions();
        var query = new SosQueryBuilder()
            .Add("includeEventIds", requestOptions.IncludeEventIds)
            .Add("validateSearchFilter", requestOptions.ValidateSearchFilter)
            .Add("skip", requestOptions.Skip)
            .Add("take", requestOptions.Take);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            query.AppendTo("DataStewardship/datasets"))
        {
            Content = SosClientHttp.CreateJsonContent(SosClientHttp.Serialize(filter))
        };
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<PagedResultDto<DsDatasetDto>>(
            response,
            request.RequestUri,
            cancellationToken);
    }
}
