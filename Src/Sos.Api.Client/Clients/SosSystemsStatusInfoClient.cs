using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Metadata.Responses;
using SOS.Api.Contracts.Status.Requests;
using SOS.Api.Contracts.Status.Responses;
using System.Text.Json;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosSystemsStatusInfoClient : ISosSystemsStatusInfoClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a system-status client using the supplied HTTP client.</summary>
    public SosSystemsStatusInfoClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<ProcessSummaryDto?> GetProcessSummaryAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<ProcessSummaryDto>("systems/process-summary", cancellationToken);

    /// <inheritdoc />
    public Task<ProcessInfoDto?> GetProcessInfoAsync(bool active, CancellationToken cancellationToken = default)
        => GetJsonAsync<ProcessInfoDto>($"Systems/ProcessInformation?active={active}", cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<MongoDbProcessInfoDto>?> GetMongoDbProcessInfoAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<IEnumerable<MongoDbProcessInfoDto>>("systems/processmongodb", cancellationToken);

    /// <inheritdoc />
    public Task<ActiveInstanceInfoDto?> GetActiveInstanceAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<ActiveInstanceInfoDto>("systems/activeinstance", cancellationToken);

    /// <inheritdoc />
    public Task<SearchIndexInfoDto?> GetSearchIndexInfoAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<SearchIndexInfoDto>("systems/searchindex", cancellationToken);

    /// <inheritdoc />
    public Task<MongoDbInfoDto?> GetMongoDatabaseInfoAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<MongoDbInfoDto>("systems/mongoinfo", cancellationToken);

    /// <inheritdoc />
    public Task<HealthReportDto?> GetHealthAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<HealthReportDto>("health", cancellationToken);

    /// <inheritdoc />
    public async Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = false,
        DiagramFormat diagramFormat = DiagramFormat.Mermaid,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taxonIds);
        if (taxonIds.Length == 0)
        {
            throw new ArgumentException("At least one taxon id must be provided.", nameof(taxonIds));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            BuildTaxonRelationsDiagramUri(
                taxonIds,
                treeIterationMode,
                includeSecondaryRelations,
                diagramFormat,
                taxonomyType,
                translationCultureCode));
        request.ApplySosRequestHeaders(acceptProfile: SosAcceptProfile.TextOrJson);

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

        var responseBody = await SosClientHttp.ReadResponseBodyAsync(response, cancellationToken);
        return ReadStringResponseContent(responseBody, response.Content.Headers.ContentType?.MediaType);
    }

    private async Task<T?> GetJsonAsync<T>(string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<T>(response, request.RequestUri, cancellationToken);
    }

    private static Uri BuildTaxonRelationsDiagramUri(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode,
        bool includeSecondaryRelations,
        DiagramFormat diagramFormat,
        TaxonomyType taxonomyType,
        string translationCultureCode)
    {
        return new Uri(
            new SosQueryBuilder()
                .AddValues("taxonIds", taxonIds)
                .AddEnum("treeIterationMode", treeIterationMode)
                .Add("includeSecondaryRelations", includeSecondaryRelations)
                .AddEnum("diagramFormat", diagramFormat)
                .AddEnum("taxonomyType", taxonomyType)
                .Add("translationCultureCode", translationCultureCode)
                .AppendTo("systems/TaxonRelationsDiagram"),
            UriKind.Relative);
    }

    private static string? ReadStringResponseContent(string responseBody, string? mediaType)
    {
        if (string.IsNullOrEmpty(responseBody) ||
            !IsJsonMediaType(mediaType))
        {
            return responseBody;
        }

        try
        {
            return JsonSerializer.Deserialize<string>(responseBody);
        }
        catch (JsonException)
        {
            return responseBody;
        }
    }

    private static bool IsJsonMediaType(string? mediaType)
    {
        return mediaType is not null &&
               (mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
    }
}
