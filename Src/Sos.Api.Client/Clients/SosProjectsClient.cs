using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosProjectsClient : ISosProjectsClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates a project client using the supplied HTTP client.</summary>
    public SosProjectsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var uri = new SosQueryBuilder()
            .Add("filter", string.IsNullOrWhiteSpace(filter) ? null : filter)
            .AppendTo("projects");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var projects = await SosClientHttp.ReadJsonResponseAsync<List<ProjectDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return projects ?? [];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProjectDto>> GetProjectsByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        using var request = new HttpRequestMessage(HttpMethod.Post, "projects")
        {
            Content = SosClientHttp.CreateJsonContent(SosClientHttp.Serialize(ids.ToArray()))
        };
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var projects = await SosClientHttp.ReadJsonResponseAsync<List<ProjectDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return projects ?? [];
    }

    /// <inheritdoc />
    public async Task<ProjectDto?> GetProjectAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"projects/{id}");
        request.ApplySosRequestHeaders();

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<ProjectDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }
}
