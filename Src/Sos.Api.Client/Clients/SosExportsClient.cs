using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Responses;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosExportsClient : ISosExportsClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates an export client using the supplied HTTP client.</summary>
    public SosExportsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<SosStreamingResponse> DownloadInternalAsync(
        SearchFilterInternalDto filter,
        SosExportType exportType,
        ExportDownloadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var requestOptions = options ?? new ExportDownloadOptions();
        using var request = CreateRequest(
            $"exports/internal/download/{Uri.EscapeDataString(exportType.ToString())}",
            BuildDownloadQuery(exportType, requestOptions),
            filter,
            requestOptions,
            SosAcceptProfile.Any);

        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            using (response)
            {
                var body = await SosClientHttp.ReadErrorResponsePreviewAsync(response, cancellationToken);
                throw SosClientHttp.CreateApiException(response, body, request.RequestUri);
            }
        }

        try
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return new SosStreamingResponse(response, stream);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> OrderInternalAsync(
        SearchFilterInternalDto filter,
        SosExportType exportType,
        ExportOrderOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Description))
        {
            throw new ArgumentException("Description must not be empty.", nameof(options));
        }

        var query = BuildDownloadQuery(exportType, options);
        query.Add("description", options.Description)
            .Add("sendMailFromZendTo", options.SendMailFromZendTo)
            .Add("encryptPassword", options.EncryptPassword)
            .Add("confirmEncryptPassword", options.ConfirmEncryptPassword);

        using var request = CreateRequest(
            $"exports/internal/order/{Uri.EscapeDataString(exportType.ToString())}",
            query,
            filter,
            options,
            SosAcceptProfile.Json);
        request.SuppressRetries();
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadStringResponseAsync(
            response,
            request.RequestUri,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExportJobInfoDto>> GetMyExportsAsync(
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestOptions = options ?? new EmptyRoleRequestOptions();
        using var request = new HttpRequestMessage(HttpMethod.Get, "exports/my");
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<List<ExportJobInfoDto>>(
                   response,
                   request.RequestUri,
                   cancellationToken)
               ?? [];
    }

    /// <inheritdoc />
    public async Task<ExportJobInfoDto?> GetMyExportAsync(
        string id,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Export id must not be empty.", nameof(id));
        }

        var requestOptions = options ?? new EmptyRoleRequestOptions();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"exports/my/{Uri.EscapeDataString(id)}");
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<ExportJobInfoDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(
        string path,
        SosQueryBuilder query,
        SearchFilterInternalDto filter,
        SosRoleRequestOptions options,
        SosAcceptProfile acceptProfile)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, query.AppendTo(path))
        {
            Content = SosClientHttp.CreateJsonContent(SosClientHttp.Serialize(filter))
        };
        request.ApplySosRequestHeaders(
            options.RoleId,
            options.AuthorizationApplicationIdentifier,
            acceptProfile);
        return request;
    }

    private static SosQueryBuilder BuildDownloadQuery(SosExportType exportType, ExportDownloadOptions options)
    {
        var query = new SosQueryBuilder()
            .Add("validateSearchFilter", options.ValidateSearchFilter)
            .Add("sensitiveObservations", options.SensitiveObservations);

        if (exportType == SosExportType.DwC)
        {
            query.Add("eventBased", options.EventBased);
            return query;
        }

        query.Add("outputFieldSet", options.OutputFieldSet.ToString())
            .Add("propertyLabelType", options.PropertyLabelType.ToString())
            .Add("cultureCode", options.CultureCode);
        if (exportType == SosExportType.Excel)
        {
            query.Add("dynamicProjectDataFields", options.DynamicProjectDataFields);
        }

        if (exportType == SosExportType.GeoJson)
        {
            query.Add("flat", options.Flat)
                .Add("excludeNullValues", options.ExcludeNullValues)
                .Add("gzip", options.Gzip);
        }
        else
        {
            query.Add("gzip", options.Gzip);
        }

        return query;
    }

    private sealed class EmptyRoleRequestOptions : SosRoleRequestOptions;
}
